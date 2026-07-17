using Application.Common.Interfaces.Services;
using Application.Common.Models;
using MediatR;

namespace Application.Features.People.Queries.GetParents;

public class GetParentsQueryHandler
    : IRequestHandler<GetParentsQuery, Result<PagedResult<ParentListDto>>>
{
    private readonly IParentReadService _parentReadService;

    public GetParentsQueryHandler(IParentReadService parentReadService)
    {
        _parentReadService = parentReadService;
    }

    public async Task<Result<PagedResult<ParentListDto>>> Handle(
        GetParentsQuery request, CancellationToken ct)
    {
        var pagination = request.Pagination ?? new PaginationParams();
        var parents = await _parentReadService.GetParentsAsync(request.SchoolId, request.SearchTerm, pagination, ct);

        return Result.Success(parents);
    }
}
