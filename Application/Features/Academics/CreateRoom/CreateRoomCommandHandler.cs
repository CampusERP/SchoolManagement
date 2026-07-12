using MediatR;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Common.Interfaces.Repositories;
using Domain.Entities.Academics;

namespace Application.Features.Academics.CreateRoom;

public class CreateRoomCommandHandler : IRequestHandler<CreateRoomCommand, Result<Guid>>
{
    private readonly IRoomRepository _rooms;
    private readonly ITenantContext _tenant;

    public CreateRoomCommandHandler(IRoomRepository rooms, ITenantContext tenant)
    {
        _rooms = rooms;
        _tenant = tenant;
    }

    public async Task<Result<Guid>> Handle(CreateRoomCommand request, CancellationToken ct)
    {
        var schoolId = _tenant.SchoolId
            ?? throw new UnauthorizedAccessException("No school context found.");

        var exists = await _rooms.ExistsAsync(schoolId, request.Name, ct);
        if (exists)
            return Result.Failure<Guid>($"A room with name '{request.Name}' already exists in this school.");

        var room = Room.Create(schoolId, request.Name, request.Capacity);
        await _rooms.AddAsync(room, ct);

        return Result.Success(room.Id);
    }
}
