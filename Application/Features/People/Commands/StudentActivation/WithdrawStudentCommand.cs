using Application.Common.Behaviors;
using Application.Common.Interfaces;

namespace Application.Features.People.Commands.StudentActivation;

public record WithdrawStudentCommand(Guid SchoolId, Guid StudentEnrollmentId)
    : ICommand, ITenantScopedRequest;