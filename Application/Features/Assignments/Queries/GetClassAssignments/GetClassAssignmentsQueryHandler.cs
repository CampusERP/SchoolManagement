using Application.Common.Interfaces.Services;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Assignments.Queries.GetClassAssignments;

public class GetClassAssignmentsQueryHandler
    : IRequestHandler<GetClassAssignmentsQuery, Result<PagedResult<AssignmentSummaryDto>>>
{
    private readonly IAssignmentReadService _readService;

    public GetClassAssignmentsQueryHandler(IAssignmentReadService readService) => _readService = readService;

    public async Task<Result<PagedResult<AssignmentSummaryDto>>> Handle(
        GetClassAssignmentsQuery request, CancellationToken ct)
    {
        var p = request.Pagination ?? new PaginationParams();
        var result = await _readService.GetClassAssignmentsAsync(request.TeachingAssignmentId, p, ct);
        return Result.Success(result);
    }
}
