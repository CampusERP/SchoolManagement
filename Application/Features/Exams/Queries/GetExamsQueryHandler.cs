using Application.Common.Interfaces.Services;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Exams.Queries;

public class GetExamsQueryHandler
    : IRequestHandler<GetExamsQuery, Result<PagedResult<ExamListDto>>>
{
    private readonly IExamReadService _readService;
    public GetExamsQueryHandler(IExamReadService readService) => _readService = readService;

    public async Task<Result<PagedResult<ExamListDto>>> Handle(
        GetExamsQuery request, CancellationToken ct)
    {
        var p = request.Pagination ?? new PaginationParams();
        var result = await _readService.GetExamsAsync(request.TermId, p, ct);
        return Result.Success(result);
    }
}
