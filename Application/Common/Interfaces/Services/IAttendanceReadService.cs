using Application.Common.Models;
using Application.Features.Attendance.Queries.GetClassAttendance;

namespace Application.Common.Interfaces.Services;

public interface IAttendanceReadService
{
    Task<ClassAttendanceDto?> GetClassAttendanceAsync(Guid classScheduleId, DateOnly date, CancellationToken ct = default);
    Task<PagedResult<StudentAttendanceSummaryDto>> GetStudentAttendanceAsync(Guid enrollmentId, PaginationParams p, CancellationToken ct = default);
}
