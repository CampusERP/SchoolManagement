using Application.Common.Interfaces.Repositories;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Academics.Commands.UpdateRoom;

public class UpdateRoomCommandHandler : IRequestHandler<UpdateRoomCommand, Result>
{
    private readonly IRoomRepository _rooms;

    public UpdateRoomCommandHandler(IRoomRepository rooms)
    {
        _rooms = rooms;
    }

    public async Task<Result> Handle(UpdateRoomCommand request, CancellationToken ct)
    {
        var room = await _rooms.GetByIdAsync(request.RoomId, ct);
        if (room is null) return Result.Failure("Room not found.");

        try
        {
            room.Update(request.Name, request.Capacity);
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.Message);
        }

        return Result.Success();
    }
}
