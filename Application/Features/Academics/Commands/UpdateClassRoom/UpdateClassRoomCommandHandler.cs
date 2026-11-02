using Application.Common.Interfaces.Repositories;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Academics.Commands.UpdateClassRoom;

public class UpdateClassRoomCommandHandler : IRequestHandler<UpdateClassRoomCommand, Result>
{
    private readonly IClassRoomRepository _classRooms;
    private readonly IRoomRepository _rooms;

    public UpdateClassRoomCommandHandler(IClassRoomRepository classRooms, IRoomRepository rooms)
    {
        _classRooms = classRooms;
        _rooms = rooms;
    }

    public async Task<Result> Handle(UpdateClassRoomCommand request, CancellationToken ct)
    {
        var classRoom = await _classRooms.GetByIdAsync(request.ClassRoomId, ct);
        if (classRoom is null || classRoom.SchoolId != request.SchoolId) return Result.Failure("Classroom not found.");

        if (request.RoomId.HasValue)
        {
            var room = await _rooms.GetByIdAsync(request.RoomId.Value, ct);
            if (room is null || room.SchoolId != request.SchoolId)
                return Result.Failure("The selected room was not found in this school.");
        }

        try
        {
            classRoom.Update(request.Name, request.RoomId);
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.Message);
        }

        return Result.Success();
    }
}
