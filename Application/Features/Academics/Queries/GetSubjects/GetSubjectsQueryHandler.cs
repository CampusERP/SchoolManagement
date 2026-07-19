using Application.Common.Interfaces.Repositories;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Academics.Queries.GetSubjects;

public class GetSubjectsQueryHandler
    : IRequestHandler<GetSubjectsQuery, Result<List<SubjectDto>>>
{
    private readonly ISubjectQueryRepository _repository;

    public GetSubjectsQueryHandler(ISubjectQueryRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<List<SubjectDto>>> Handle(
        GetSubjectsQuery request,
        CancellationToken cancellationToken)
    {
        var subjects = await _repository.GetSubjectsAsync(
            request.GradeLevelId,
            cancellationToken);

        return Result.Success(subjects);
    }
}