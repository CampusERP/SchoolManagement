using Application.Common.Interfaces.Repositories;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Academics.Commands.UpdateRoom;

public class UpdateRoomCommandHandler : IRequestHandler<UpdateRoomCommand, Result>
{
    private readonly IRoomRepository _rooms;
    private readonly ISchoolRepository _schools;

    public UpdateRoomCommandHandler(IRoomRepository rooms, ISchoolRepository schools)
    {
        _rooms = rooms;
        _schools = schools;
    }

    public async Task<Result> Handle(UpdateRoomCommand request, CancellationToken ct)
    {
        var room = await _rooms.GetByIdAsync(request.RoomId, ct);
        if (room is null) return Result.Failure("Room not found.");
        var campuses = await _schools.GetCampusesAsync(request.SchoolId, ct);
        if (campuses.Count > 0 && !request.CampusId.HasValue)
            return Result.Failure("Select a campus for this room.");
        if (request.CampusId.HasValue && await _schools.GetCampusAsync(request.SchoolId, request.CampusId.Value, ct) is null)
            return Result.Failure("The selected campus was not found in this school.");

        try
        {
            room.Update(request.Name, request.Capacity, request.CampusId);
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.Message);
        }

        return Result.Success();
    }
}
