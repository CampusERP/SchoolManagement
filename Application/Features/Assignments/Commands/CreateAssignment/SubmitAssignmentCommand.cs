using MediatR;
using Application.Common.Interfaces;
using Application.Common.Models;

namespace Application.Features.Assignments.Commands.CreateAssignment;

public record SubmitAssignmentCommand(
    Guid          SchoolId,
    Guid          AssignmentId,
    Guid          StudentEnrollmentId,
    List<SubmissionFile>? Files = null)
    : ICommand<Guid>, ITenantScopedRequest;
