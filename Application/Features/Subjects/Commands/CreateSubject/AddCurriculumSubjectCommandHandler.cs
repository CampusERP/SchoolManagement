using MediatR;
using Application.Common.Models;
using Domain.Entities.Academics;
using Application.Common.Interfaces.Repositories;

namespace Application.Features.Subjects.Commands.CreateSubject;

public class AddCurriculumSubjectCommandHandler : IRequestHandler<AddCurriculumSubjectCommand, Result<Guid>>
{
    private readonly ICurriculumSubjectRepository _repository;

    public AddCurriculumSubjectCommandHandler(ICurriculumSubjectRepository repository) => _repository = repository;

    public async Task<Result<Guid>> Handle(AddCurriculumSubjectCommand request, CancellationToken ct)
    {
        var already = await _repository.ExistsAsync(request.GradeLevelId, request.SubjectId, ct);

        if (already)
            return Result.Failure<Guid>("This subject is already assigned to this grade level.");

        var cs = CurriculumSubject.Create(request.SchoolId, request.GradeLevelId, request.SubjectId);
        _repository.Add(cs);
        return Result.Success(cs.Id);
    }
}
