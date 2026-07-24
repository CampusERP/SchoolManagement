namespace Application.Features.Schools.Queries.GetSchoolDashboard;

public record SchoolDashboardDto(
    string SchoolName,
    int TotalStudents,
    int TotalTeachers,
    int TotalParents,
    int TotalClassRooms,
    int ActiveEnrollments,
    string? CurrentAcademicYear,
    List<RecentStudentDto> RecentStudents,
    AttendanceSummaryDto AttendanceSummary);

public record RecentStudentDto(
    Guid Id,
    string Name,
    string Code,
    string GradeLevel,
    DateTime EnrolledAt,
    string Status);

public record AttendanceSummaryDto(int Present, int Absent, int Late, int Excused);
