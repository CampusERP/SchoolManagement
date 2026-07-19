using MediatR;
using Application.Common.Models;
using Application.Common.Interfaces.Repositories;
using Domain.Entities.Academics;

namespace Application.Features.Academics.Commands.CreateRoom;

public class CreateRoomCommandHandler : IRequestHandler<CreateRoomCommand, Result<Guid>>
{
    private readonly IRoomRepository _rooms;

    public CreateRoomCommandHandler(IRoomRepository rooms)
    {
        _rooms = rooms;
    }

    public async Task<Result<Guid>> Handle(CreateRoomCommand request, CancellationToken ct)
    {
        var exists = await _rooms.ExistsAsync(request.SchoolId, request.Name, ct);
        if (exists)
            return Result.Failure<Guid>($"A room with name '{request.Name}' already exists in this school.");

        var room = Room.Create(request.SchoolId, request.Name, request.Capacity);
        await _rooms.AddAsync(room, ct);

        return Result.Success(room.Id);
    }
}