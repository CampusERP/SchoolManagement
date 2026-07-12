using Application.Common.Behaviors;

namespace Application.Features.Enrollment.Commands.EnrollStudent;

public record EnrollStudentCommand(
    Guid StudentId,
    Guid ClassRoomId,
    Guid AcademicYearId) : ICommand<Guid>, IBaseCommand;
