using Application.Common.Interfaces.Services;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Exams.Queries;

public class GetStudentExamResultsQueryHandler
    : IRequestHandler<GetStudentExamResultsQuery, Result<PagedResult<ExamResultDto>>>
{
    private readonly IExamReadService _readService;
    public GetStudentExamResultsQueryHandler(IExamReadService readService) => _readService = readService;

    public async Task<Result<PagedResult<ExamResultDto>>> Handle(
        GetStudentExamResultsQuery request, CancellationToken ct)
    {
        var p = request.Pagination ?? new PaginationParams();
        var result = await _readService.GetStudentExamResultsAsync(request.StudentEnrollmentId, request.TermId, p, ct);

        return Result.Success(result);
    }
}
