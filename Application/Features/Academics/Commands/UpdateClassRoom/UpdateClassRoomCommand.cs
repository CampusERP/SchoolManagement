using Application.Common.Behaviors;
using Application.Common.Interfaces;

namespace Application.Features.Academics.Commands.UpdateClassRoom;

public record UpdateClassRoomCommand(
    Guid SchoolId,
    Guid ClassRoomId,
    string Name,
    Guid? RoomId = null,
    Guid? CampusId = null) : ICommand, IBaseCommand, ITenantScopedRequest;
