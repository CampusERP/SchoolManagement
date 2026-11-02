using Application.Common.Interfaces.Repositories;
using Application.Common.Models;
using MediatR;

namespace Application.Features.SchoolRegistrationRequests.Queries.GetSchoolRegistrationRequests;

public class GetSchoolRegistrationRequestsQueryHandler
    : IRequestHandler<GetSchoolRegistrationRequestsQuery, Result<PagedResult<SchoolRegistrationRequestDto>>>
{
    private readonly ISchoolRegistrationRequestRepository _requests;

    public GetSchoolRegistrationRequestsQueryHandler(ISchoolRegistrationRequestRepository requests)
    {
        _requests = requests;
    }

    public async Task<Result<PagedResult<SchoolRegistrationRequestDto>>> Handle(
        GetSchoolRegistrationRequestsQuery request, CancellationToken ct)
    {
        var p = request.Pagination ?? new PaginationParams();

        var (items, total) = await _requests.GetPagedAsync(
            p.Page, p.PageSize, request.Search, request.Status, ct);

        var dtos = items.Select(SchoolRegistrationRequestDto.FromEntity).ToList();

        return Result.Success(new PagedResult<SchoolRegistrationRequestDto>(dtos, total, p.Page, p.PageSize));
    }
}
