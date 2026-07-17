namespace Application.Features.Schools.Queries.GetSchoolDashboard;

public record SchoolDashboardDto(
    string SchoolName,
    int TotalStudents,
    int TotalTeachers,
    int TotalParents,
    int TotalClassRooms,
    int ActiveEnrollments,
    string? CurrentAcademicYear);
