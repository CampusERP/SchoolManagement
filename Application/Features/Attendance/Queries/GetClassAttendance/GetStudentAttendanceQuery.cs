using Application.Common.Interfaces;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Attendance.Queries.GetClassAttendance;

public record GetStudentAttendanceQuery(
    Guid SchoolId,
    Guid StudentEnrollmentId,
    Guid? AcademicYearId = null,
    PaginationParams? Pagination = null)
    : IRequest<Result<PagedResult<StudentAttendanceSummaryDto>>>, ITenantScopedRequest;
