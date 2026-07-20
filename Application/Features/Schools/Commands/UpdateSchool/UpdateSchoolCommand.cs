using Application.Common.Behaviors;
using Application.Common.Interfaces;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Schools.Commands.UpdateSchool;

public record UpdateSchoolCommand(
    Guid SchoolId,
    string Name,
    string? Address = null,
    string? Phone = null,
    string? Email = null)
    : ICommand, ITenantScopedRequest;
