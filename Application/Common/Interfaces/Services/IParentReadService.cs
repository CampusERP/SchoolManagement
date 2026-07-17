using Application.Common.Models;
using Application.Features.People.Queries.GetMyChildren;
using Application.Features.People.Queries.GetParents;

namespace Application.Common.Interfaces.Services;

public interface IParentReadService
{
    Task<PagedResult<ParentListDto>> GetParentsAsync(Guid schoolId, string? searchTerm, PaginationParams pagination, CancellationToken ct);
    Task<ParentDetailDto?> GetParentByIdAsync(Guid schoolId, Guid parentId, CancellationToken ct);
    Task<List<ChildSummaryDto>> GetMyChildrenAsync(Guid schoolId, Guid userId, CancellationToken ct);
}
