namespace Application.Features.Schools.Queries.GetPlatformAnalytics;

public record PlatformAnalyticsDto(
    int TotalSchools,
    int ActiveSchools,
    int SuspendedSchools,
    int TotalUsers);
