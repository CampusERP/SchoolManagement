using Application.Common.Interfaces.Repositories;
using Application.Common.Models;
using Domain.Entities.Academics;
using MediatR;

namespace Application.Features.Subjects.Commands.ManageSchoolSubjects;

public sealed class CreateSchoolSubjectCommandHandler : IRequestHandler<CreateSchoolSubjectCommand, Result<Guid>>
{
    private readonly ISubjectRepository _subjects;
    private readonly IGradeLevelRepository _grades;
    private readonly ICurriculumSubjectRepository _curriculum;

    public CreateSchoolSubjectCommandHandler(ISubjectRepository subjects, IGradeLevelRepository grades, ICurriculumSubjectRepository curriculum)
    {
        _subjects = subjects;
        _grades = grades;
        _curriculum = curriculum;
    }

    public async Task<Result<Guid>> Handle(CreateSchoolSubjectCommand request, CancellationToken ct)
    {
        var grade = await _grades.GetByIdAsync(request.GradeLevelId, ct);
        if (grade is null || grade.SchoolId != request.SchoolId)
            return Result.Failure<Guid>("Grade level not found.");

        var code = request.Code.Trim().ToUpperInvariant();
        if (await _subjects.ExistsForSchoolAsync(code, request.SchoolId, ct: ct))
            return Result.Failure<Guid>($"A subject with code '{code}' already exists in this school.");

        var subject = Subject.Create(request.SchoolId, code, request.Name, request.Description);
        _subjects.Add(subject);
        var assignment = CurriculumSubject.Create(request.SchoolId, request.GradeLevelId, subject.Id);
        _curriculum.Add(assignment);
        return Result.Success(assignment.Id);
    }
}

public sealed class UpdateSchoolSubjectCommandHandler : IRequestHandler<UpdateSchoolSubjectCommand, Result>
{
    private readonly ISubjectRepository _subjects;
    private readonly ICurriculumSubjectRepository _curriculum;

    public UpdateSchoolSubjectCommandHandler(ISubjectRepository subjects, ICurriculumSubjectRepository curriculum)
    {
        _subjects = subjects;
        _curriculum = curriculum;
    }

    public async Task<Result> Handle(UpdateSchoolSubjectCommand request, CancellationToken ct)
    {
        var assignment = await _curriculum.GetByIdAsync(request.CurriculumSubjectId, ct);
        if (assignment is null || assignment.SchoolId != request.SchoolId)
            return Result.Failure("Subject assignment not found.");

        var current = await _subjects.GetByIdAsync(assignment.SubjectId, ct);
        if (current is null)
            return Result.Failure("Subject not found.");

        var code = request.Code.Trim().ToUpperInvariant();
        if (await _subjects.ExistsForSchoolAsync(code, request.SchoolId, current.SchoolId == request.SchoolId ? current.Id : null, ct))
            return Result.Failure($"A subject with code '{code}' already exists in this school.");

        if (current.SchoolId == request.SchoolId)
        {
            current.Update(code, request.Name, request.Description);
        }
        else if (current.SchoolId is null)
        {
            // Editing a shared catalog subject in one grade creates a school-owned copy.
            var schoolCopy = Subject.Create(request.SchoolId, code, request.Name, request.Description);
            _subjects.Add(schoolCopy);
            assignment.ChangeSubject(schoolCopy.Id);
        }
        else
        {
            return Result.Failure("This subject belongs to a different school.");
        }

        return Result.Success();
    }
}

public sealed class DeleteSchoolSubjectCommandHandler : IRequestHandler<DeleteSchoolSubjectCommand, Result>
{
    private readonly ISubjectRepository _subjects;
    private readonly ICurriculumSubjectRepository _curriculum;
    private readonly ISubjectBookRepository _books;

    public DeleteSchoolSubjectCommandHandler(ISubjectRepository subjects, ICurriculumSubjectRepository curriculum, ISubjectBookRepository books)
    {
        _subjects = subjects;
        _curriculum = curriculum;
        _books = books;
    }

    public async Task<Result> Handle(DeleteSchoolSubjectCommand request, CancellationToken ct)
    {
        var subject = await _subjects.GetByIdAsync(request.SubjectId, ct);
        if (subject is null || subject.SchoolId != request.SchoolId)
            return Result.Failure("School subject not found.");
        if (await _subjects.HasAcademicUsageAsync(subject.Id, request.SchoolId, ct))
            return Result.Failure("This subject is used by teaching assignments or academic records and cannot be deleted.");

        foreach (var assignment in await _curriculum.GetBySubjectAsync(subject.Id, ct))
        {
            foreach (var book in await _books.GetByCurriculumSubjectAsync(assignment.Id, ct))
                _books.Remove(book);
            _curriculum.Remove(assignment);
        }
        _subjects.Remove(subject);
        return Result.Success();
    }
}
