using Domain.Enums;

namespace Application.Features.Attendance.Queries.GetClassAttendance;

public record StudentAttendanceSummaryDto(
    DateOnly Date,
    string   SubjectName,
    string   SubjectCode,
    AttendanceStatus Status,
    string?  Note);
