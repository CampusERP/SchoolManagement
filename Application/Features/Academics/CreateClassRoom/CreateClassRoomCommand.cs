using Application.Common.Behaviors;
using Application.Common.Interfaces;

namespace Application.Features.Academics.CreateClassRoom;

public record CreateClassRoomCommand(
    Guid SchoolId,
    Guid GradeLevelId,
    Guid AcademicYearId,
    string Name) : ICommand<Guid>, IBaseCommand, ITenantScopedRequest;
