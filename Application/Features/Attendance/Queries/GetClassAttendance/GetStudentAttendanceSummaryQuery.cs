using Application.Common.Interfaces;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Attendance.Queries.GetClassAttendance;

public record GetStudentAttendanceSummaryQuery(
    Guid SchoolId,
    Guid StudentEnrollmentId)
    : IRequest<Result<StudentAttendanceSummaryResponse>>, ITenantScopedRequest;
