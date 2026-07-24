namespace Application.Features.Schools.Queries.GetPlatformAnalytics;

public record PlatformAnalyticsDto(
    int TotalSchools,
    int ActiveSchools,
    int SuspendedSchools,
    int TotalStudents,
    int TotalParents,
    int TotalTeachers,
    int TotalSchoolAdmins,
    int TotalUsers);
