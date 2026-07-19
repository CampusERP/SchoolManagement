using Application.Common.Interfaces;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Attendance.Queries.GetClassAttendance;

public record GetClassAttendanceQuery(
    Guid SchoolId,
    Guid ClassScheduleId,
    DateOnly Date)
    : IRequest<Result<ClassAttendanceDto>>, ITenantScopedRequest;
