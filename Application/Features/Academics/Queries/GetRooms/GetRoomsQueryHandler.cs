using Application.Common.Interfaces.Services;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Academics.Queries.GetRooms;

public class GetRoomsQueryHandler
    : IRequestHandler<GetRoomsQuery, Result<List<RoomDto>>>
{
    private readonly IAcademicReadService _academicReadService;
    public GetRoomsQueryHandler(IAcademicReadService academicReadService) => _academicReadService = academicReadService;

    public async Task<Result<List<RoomDto>>> Handle(
        GetRoomsQuery request, CancellationToken ct)
    {
        var items = await _academicReadService.GetRoomsAsync(ct);
        return Result.Success(items);
    }
}
