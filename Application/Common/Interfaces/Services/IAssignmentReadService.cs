using Application.Common.Models;
using Application.Features.Assignments.Queries.GetClassAssignments;

namespace Application.Common.Interfaces.Services;

public interface IAssignmentReadService
{
    Task<PagedResult<AssignmentSummaryDto>> GetClassAssignmentsAsync(Guid teachingAssignmentId, PaginationParams p, CancellationToken ct = default);
    Task<PagedResult<StudentAssignmentDto>> GetStudentAssignmentsAsync(Guid enrollmentId, PaginationParams p, CancellationToken ct = default);
}
