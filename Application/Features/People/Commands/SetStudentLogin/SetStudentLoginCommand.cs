using Application.Common.Behaviors;
using Application.Common.Interfaces;

namespace Application.Features.People.Commands.SetStudentLogin;

public sealed record SetStudentLoginCommand(
    Guid SchoolId,
    Guid StudentId,
    bool Enabled,
    string? Email = null,
    string? Password = null) : ICommand, ITenantScopedRequest;
