using Application.Common.Interfaces.Services;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Portal.Queries.GetTeacherSchedule;

public class GetClassRoomRosterQueryHandler
    : IRequestHandler<GetClassRoomRosterQuery, Result<PagedResult<RosterStudentDto>>>
{
    private readonly IPortalReadService _readService;
    public GetClassRoomRosterQueryHandler(IPortalReadService readService) => _readService = readService;

    public async Task<Result<PagedResult<RosterStudentDto>>> Handle(
        GetClassRoomRosterQuery request, CancellationToken ct)
    {
        var p = request.Pagination ?? new PaginationParams();
        var roster = await _readService.GetClassRoomRosterAsync(request.ClassRoomId, p.Page, p.PageSize, ct);
        
        return Result.Success(roster);
    }
}
