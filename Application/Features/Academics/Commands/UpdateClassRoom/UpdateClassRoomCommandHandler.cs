using Application.Common.Interfaces.Repositories;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Academics.Commands.UpdateClassRoom;

public class UpdateClassRoomCommandHandler : IRequestHandler<UpdateClassRoomCommand, Result>
{
    private readonly IClassRoomRepository _classRooms;
    private readonly IRoomRepository _rooms;
    private readonly ISchoolRepository _schools;

    public UpdateClassRoomCommandHandler(IClassRoomRepository classRooms, IRoomRepository rooms, ISchoolRepository schools)
    {
        _classRooms = classRooms;
        _rooms = rooms;
        _schools = schools;
    }

    public async Task<Result> Handle(UpdateClassRoomCommand request, CancellationToken ct)
    {
        var classRoom = await _classRooms.GetByIdAsync(request.ClassRoomId, ct);
        if (classRoom is null || classRoom.SchoolId != request.SchoolId) return Result.Failure("Classroom not found.");
        var campuses = await _schools.GetCampusesAsync(request.SchoolId, ct);
        if (campuses.Count > 0 && !request.CampusId.HasValue)
            return Result.Failure("Select a campus for this classroom.");
        if (request.CampusId.HasValue && await _schools.GetCampusAsync(request.SchoolId, request.CampusId.Value, ct) is null)
            return Result.Failure("The selected campus was not found in this school.");

        if (request.RoomId.HasValue)
        {
            var room = await _rooms.GetByIdAsync(request.RoomId.Value, ct);
            if (room is null || room.SchoolId != request.SchoolId)
                return Result.Failure("The selected room was not found in this school.");
            if (request.CampusId.HasValue && room.CampusId != request.CampusId)
                return Result.Failure("The selected room must belong to the classroom campus.");
        }

        try
        {
            classRoom.Update(request.Name, request.RoomId, request.CampusId);
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.Message);
        }

        return Result.Success();
    }
}
