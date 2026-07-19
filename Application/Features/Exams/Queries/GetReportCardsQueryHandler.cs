using Application.Common.Interfaces.Services;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Exams.Queries;

public class GetReportCardsQueryHandler
    : IRequestHandler<GetReportCardsQuery, Result<PagedResult<ReportCardDto>>>
{
    private readonly IExamReadService _readService;
    public GetReportCardsQueryHandler(IExamReadService readService) => _readService = readService;

    public async Task<Result<PagedResult<ReportCardDto>>> Handle(
        GetReportCardsQuery request, CancellationToken ct)
    {
        var p = request.Pagination ?? new PaginationParams();
        var result = await _readService.GetReportCardsAsync(request.StudentEnrollmentId, p, ct);
        return Result.Success(result);
    }
}
