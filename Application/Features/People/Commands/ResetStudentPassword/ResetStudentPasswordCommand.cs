using Application.Common.Behaviors;
using Application.Common.Interfaces;

namespace Application.Features.People.Commands.ResetStudentPassword;

public record ResetStudentPasswordCommand(Guid SchoolId, Guid StudentId)
    : ICommand, IBaseCommand, ITenantScopedRequest;
