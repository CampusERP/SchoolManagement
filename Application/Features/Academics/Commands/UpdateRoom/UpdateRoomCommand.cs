using Application.Common.Behaviors;
using Application.Common.Interfaces;

namespace Application.Features.Academics.Commands.UpdateRoom;

public record UpdateRoomCommand(
    Guid SchoolId,
    Guid RoomId,
    string Name,
    int Capacity) : ICommand, IBaseCommand, ITenantScopedRequest;
