using Application.Common.Behaviors;
using Application.Common.Interfaces;

namespace Application.Features.Academics.Commands.CreateClassRoom;

public record CreateClassRoomCommand(
    Guid SchoolId,
    Guid GradeLevelId,
    Guid AcademicYearId,
    string Name) : ICommand<Guid>, IBaseCommand, ITenantScopedRequest;
