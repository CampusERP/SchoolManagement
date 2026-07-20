using Application.Common.Behaviors;
using Application.Common.Interfaces;
using Application.Common.Models;
using MediatR;

namespace Application.Features.People.Commands.TerminateTeacher;

public record TerminateTeacherCommand(Guid SchoolId, Guid TeacherId)
    : ICommand, ITenantScopedRequest;
