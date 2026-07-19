using MediatR;
using Application.Common.Interfaces;
using Application.Common.Models;

namespace Application.Features.Assignments.Commands.CreateAssignment;

public record CreateAssignmentCommand(
    Guid    SchoolId,
    Guid    TeachingAssignmentId,
    string  Title,
    string? Instructions,
    DateTime DueDate,
    decimal? MaxScore = null)
    : ICommand<Guid>, ITenantScopedRequest;
