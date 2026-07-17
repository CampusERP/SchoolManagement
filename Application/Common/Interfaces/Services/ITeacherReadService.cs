using Application.Common.Models;
using Application.Features.People.Queries.GetMyClasses;
using Application.Features.People.Queries.GetTeacherById;
using Application.Features.People.Queries.GetTeachers;

namespace Application.Common.Interfaces.Services;

public interface ITeacherReadService
{
    Task<PagedResult<TeacherListDto>> GetTeachersAsync(Guid schoolId, string? searchTerm, PaginationParams pagination, CancellationToken ct);
    Task<TeacherDetailDto?> GetTeacherByIdAsync(Guid schoolId, Guid teacherId, CancellationToken ct);
    Task<List<TeachingAssignmentSummaryDto>> GetMyClassesAsync(Guid schoolId, Guid userId, Guid termId, CancellationToken ct);
}
