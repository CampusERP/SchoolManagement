using Application.Common.Models;
using Application.Features.Portal.Queries.GetParentDashboard;
using Application.Features.Portal.Queries.GetParentChildAttendance;
using Application.Features.Portal.Queries.GetParentChildProfile;
using Application.Features.Portal.Queries.GetStudentClasses;
using Application.Features.Portal.Queries.GetStudentDashboard;
using Application.Features.Portal.Queries.GetTeacherDashboard;
using Application.Features.Portal.Queries.GetTeacherSchedule;
using Application.Features.Portal.Queries.Shared;

namespace Application.Common.Interfaces.Services;

public interface IPortalReadService
{
    // ── Teacher Portal ──────────────────────────────────
    Task<TeacherDashboardDto> GetTeacherDashboardAsync(Guid schoolId, Guid userId, CancellationToken ct = default);
    Task<List<TeacherScheduleSlotDto>> GetTeacherScheduleAsync(Guid teacherId, Guid termId, CancellationToken ct = default);
    Task<PagedResult<RosterStudentDto>> GetClassRoomRosterAsync(Guid classRoomId, int page, int pageSize, CancellationToken ct = default);

    // ── Student Portal ──────────────────────────────────
    Task<StudentDashboardDto?> GetStudentDashboardAsync(Guid schoolId, Guid userId, Guid enrollmentId, CancellationToken ct = default);
    Task<StudentSummaryDto?> GetStudentSummaryAsync(Guid enrollmentId, CancellationToken ct = default);
    Task<List<StudentScheduleSlotDto>> GetStudentScheduleAsync(Guid enrollmentId, Guid termId, CancellationToken ct = default);
    Task<List<StudentClassDto>> GetStudentClassesAsync(Guid schoolId, Guid userId, Guid enrollmentId, CancellationToken ct = default);
    Task<List<PortalNotificationDto>> GetStudentNotificationsAsync(Guid userId, int limit, CancellationToken ct = default);

    // ── Parent Portal ───────────────────────────────────
    Task<ParentDashboardDto> GetParentDashboardAsync(Guid schoolId, Guid userId, CancellationToken ct = default);
    Task<ParentChildProfileDto?> GetParentChildProfileAsync(Guid schoolId, Guid userId, Guid studentId, CancellationToken ct = default);
    Task<PagedResult<PortalAttendanceRecordDto>> GetParentChildAttendanceAsync(Guid schoolId, Guid userId, Guid studentId, PaginationParams p, CancellationToken ct = default);
    Task<PagedResult<PortalExamResultDto>> GetParentChildGradesAsync(Guid schoolId, Guid userId, Guid studentId, Guid? termId, PaginationParams p, CancellationToken ct = default);
    Task<PagedResult<PortalAssignmentDto>> GetParentChildAssignmentsAsync(Guid schoolId, Guid userId, Guid studentId, PaginationParams p, CancellationToken ct = default);
    Task<PagedResult<PortalReportCardDto>> GetParentChildReportCardsAsync(Guid schoolId, Guid userId, Guid studentId, PaginationParams p, CancellationToken ct = default);
    Task<PagedResult<PortalInvoiceDto>> GetParentBillingAsync(Guid schoolId, PaginationParams p, CancellationToken ct = default);
    Task<List<PortalNotificationDto>> GetParentNotificationsAsync(Guid userId, int limit, CancellationToken ct = default);
}
