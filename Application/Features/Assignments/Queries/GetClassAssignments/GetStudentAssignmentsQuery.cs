using MediatR;
using Application.Common.Interfaces;
using Application.Common.Models;

namespace Application.Features.Assignments.Queries.GetClassAssignments;

public record GetStudentAssignmentsQuery(
    Guid SchoolId,
    Guid StudentEnrollmentId,
    PaginationParams? Pagination = null)
    : IRequest<Result<PagedResult<StudentAssignmentDto>>>, ITenantScopedRequest;
