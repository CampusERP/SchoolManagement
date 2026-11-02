using MediatR;
using Application.Common.Models;
using Domain.Entities.Academics;
using Application.Common.Interfaces.Repositories;

namespace Application.Features.Subjects.Commands.CreateSubject;

public class AddCurriculumSubjectCommandHandler : IRequestHandler<AddCurriculumSubjectCommand, Result<Guid>>
{
    private readonly ICurriculumSubjectRepository _repository;
    private readonly IGradeLevelRepository _gradeLevels;
    private readonly ISubjectRepository _subjects;

    public AddCurriculumSubjectCommandHandler(
        ICurriculumSubjectRepository repository,
        IGradeLevelRepository gradeLevels,
        ISubjectRepository subjects)
    {
        _repository = repository;
        _gradeLevels = gradeLevels;
        _subjects = subjects;
    }

    public async Task<Result<Guid>> Handle(AddCurriculumSubjectCommand request, CancellationToken ct)
    {
        var grade = await _gradeLevels.GetByIdAsync(request.GradeLevelId, ct);
        if (grade is null || grade.SchoolId != request.SchoolId)
            return Result.Failure<Guid>("Grade level not found.");

        if (await _subjects.GetByIdAsync(request.SubjectId, ct) is null)
            return Result.Failure<Guid>("Subject not found.");

        var already = await _repository.ExistsAsync(request.GradeLevelId, request.SubjectId, ct);

        if (already)
            return Result.Failure<Guid>("This subject is already assigned to this grade level.");

        var cs = CurriculumSubject.Create(request.SchoolId, request.GradeLevelId, request.SubjectId);
        _repository.Add(cs);
        return Result.Success(cs.Id);
    }
}
