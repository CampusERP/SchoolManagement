using Domain.Enums;

namespace Application.Features.Attendance.Commands.RecordAttendance;

public record StudentAttendanceEntry(Guid StudentEnrollmentId, AttendanceStatus Status, string? Note = null);
