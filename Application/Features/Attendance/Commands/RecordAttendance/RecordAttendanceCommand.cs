using MediatR;
using Application.Common.Interfaces;
using Application.Common.Models;

namespace Application.Features.Attendance.Commands.RecordAttendance;

public record RecordAttendanceCommand(
    Guid SchoolId,
    Guid ClassScheduleId,
    DateOnly Date,
    List<StudentAttendanceEntry> Entries)
    : ICommand<Guid>, ITenantScopedRequest;
