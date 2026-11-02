using Application.Common.Behaviors;
using Application.Common.Interfaces;

namespace Application.Features.Academics.Commands.CreateClassRoom;

public record CreateClassRoomCommand(
    Guid SchoolId,
    Guid GradeLevelId,
    Guid AcademicYearId,
    string Name,
    Guid? RoomId = null) : ICommand<Guid>, IBaseCommand, ITenantScopedRequest;
