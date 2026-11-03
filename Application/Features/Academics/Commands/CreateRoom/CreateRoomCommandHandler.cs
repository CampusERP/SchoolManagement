using MediatR;
using Application.Common.Models;
using Application.Common.Interfaces.Repositories;
using Domain.Entities.Academics;

namespace Application.Features.Academics.Commands.CreateRoom;

public class CreateRoomCommandHandler : IRequestHandler<CreateRoomCommand, Result<Guid>>
{
    private readonly IRoomRepository _rooms;
    private readonly ISchoolRepository _schools;

    public CreateRoomCommandHandler(IRoomRepository rooms, ISchoolRepository schools)
    {
        _rooms = rooms;
        _schools = schools;
    }

    public async Task<Result<Guid>> Handle(CreateRoomCommand request, CancellationToken ct)
    {
        var campuses = await _schools.GetCampusesAsync(request.SchoolId, ct);
        if (campuses.Count > 0 && !request.CampusId.HasValue)
            return Result.Failure<Guid>("Select a campus for this room.");
        if (request.CampusId.HasValue && await _schools.GetCampusAsync(request.SchoolId, request.CampusId.Value, ct) is null)
            return Result.Failure<Guid>("The selected campus was not found in this school.");
        var exists = await _rooms.ExistsAsync(request.SchoolId, request.Name, ct);
        if (exists)
            return Result.Failure<Guid>($"A room with name '{request.Name}' already exists in this school.");

        var room = Room.Create(request.SchoolId, request.Name, request.Capacity, request.CampusId);
        await _rooms.AddAsync(room, ct);

        return Result.Success(room.Id);
    }
}
