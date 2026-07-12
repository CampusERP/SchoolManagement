using Application.Common.Behaviors;

namespace Application.Features.Academics.CreateClassRoom;

public record CreateClassRoomCommand(
    Guid GradeLevelId,
    Guid AcademicYearId,
    string Name) : ICommand<Guid>, IBaseCommand;
