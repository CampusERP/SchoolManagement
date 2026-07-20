using Application.Common.Interfaces;
using Application.Common.Models;
using Domain.Enums;
using MediatR;

namespace Application.Features.Attendance.Queries.GetClassAttendance;

public record GetStudentAttendanceQuery(
    Guid SchoolId,
    Guid StudentEnrollmentId,
    Guid? AcademicYearId = null,
    DateOnly? DateFrom = null,
    DateOnly? DateTo = null,
    AttendanceStatus? Status = null,
    PaginationParams? Pagination = null)
    : IRequest<Result<PagedResult<StudentAttendanceSummaryDto>>>, ITenantScopedRequest;
