using Application.Common.Behaviors;
using Application.Common.Interfaces;

namespace Application.Features.People.Commands.DeleteTeacher;

public record DeleteTeacherCommand(Guid SchoolId, Guid TeacherId)
    : ICommand, ITenantScopedRequest;
