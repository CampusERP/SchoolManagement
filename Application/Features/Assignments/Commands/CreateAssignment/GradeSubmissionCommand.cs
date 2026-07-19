using MediatR;
using Application.Common.Interfaces;
using Application.Common.Models;

namespace Application.Features.Assignments.Commands.CreateAssignment;

public record GradeSubmissionCommand(
    Guid    SchoolId,
    Guid    AssignmentId,
    Guid    SubmissionId,
    decimal Grade,
    string? Feedback = null)
    : ICommand, ITenantScopedRequest;
