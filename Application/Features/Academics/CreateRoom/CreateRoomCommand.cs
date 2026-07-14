using Application.Common.Behaviors;
using Application.Common.Interfaces;

namespace Application.Features.Academics.CreateRoom;

public record CreateRoomCommand(
    Guid SchoolId,
    string Name,
    int Capacity) : ICommand<Guid>, IBaseCommand, ITenantScopedRequest;
