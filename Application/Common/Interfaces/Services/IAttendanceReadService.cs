using Application.Common.Models;
using Application.Features.Attendance.Queries.GetClassAttendance;
using Domain.Enums;

namespace Application.Common.Interfaces.Services;

public interface IAttendanceReadService
{
    Task<ClassAttendanceDto?> GetClassAttendanceAsync(Guid classScheduleId, DateOnly date, CancellationToken ct = default);
    Task<PagedResult<StudentAttendanceSummaryDto>> GetStudentAttendanceAsync(
        Guid enrollmentId, Guid? academicYearId, DateOnly? dateFrom, DateOnly? dateTo,
        AttendanceStatus? status, PaginationParams p, CancellationToken ct = default);
    Task<StudentAttendanceSummaryResponse?> GetStudentAttendanceSummaryAsync(
        Guid enrollmentId, CancellationToken ct = default);
}
