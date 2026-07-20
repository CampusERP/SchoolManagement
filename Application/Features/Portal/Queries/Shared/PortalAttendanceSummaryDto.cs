namespace Application.Features.Portal.Queries.Shared;

public record PortalAttendanceSummaryDto(
    int     TotalDays,
    int     PresentDays,
    int     AbsentDays,
    int     LateDays,
    decimal AttendancePercentage);
