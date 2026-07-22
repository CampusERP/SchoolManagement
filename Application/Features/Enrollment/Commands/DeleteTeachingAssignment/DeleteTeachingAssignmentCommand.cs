using Application.Common.Behaviors;
using Application.Common.Interfaces;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Enrollment.Commands.DeleteTeachingAssignment;

public record DeleteTeachingAssignmentCommand(Guid SchoolId, Guid TeachingAssignmentId)
    : ICommand<Guid>, IBaseCommand, ITenantScopedRequest;
