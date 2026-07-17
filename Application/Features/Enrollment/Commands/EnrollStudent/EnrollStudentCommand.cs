using Application.Common.Behaviors;
using Application.Common.Interfaces;

namespace Application.Features.Enrollment.Commands.EnrollStudent;

public record EnrollStudentCommand(
    Guid SchoolId,
    Guid StudentId,
    Guid ClassRoomId,
    Guid AcademicYearId) : ICommand<Guid>, IBaseCommand, ITenantScopedRequest;
