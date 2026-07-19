using Application.Common.Models;
using Application.Features.Portal.Queries.GetTeacherSchedule;

namespace Application.Common.Interfaces.Services;

public interface IPortalReadService
{
    Task<List<TeacherScheduleSlotDto>> GetTeacherScheduleAsync(Guid teacherId, Guid termId, CancellationToken ct = default);
    Task<StudentSummaryDto?> GetStudentSummaryAsync(Guid enrollmentId, CancellationToken ct = default);
    Task<List<StudentScheduleSlotDto>> GetStudentScheduleAsync(Guid enrollmentId, Guid termId, CancellationToken ct = default);
    Task<PagedResult<RosterStudentDto>> GetClassRoomRosterAsync(Guid classRoomId, int page, int pageSize, CancellationToken ct = default);
}
