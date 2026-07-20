namespace Application.Features.Portal.Queries.GetParentDashboard;

public record ParentDashboardDto(
    List<ParentChildDashboardDto> Children,
    int     UnreadNotificationCount,
    int     OutstandingInvoiceCount,
    decimal TotalBalanceDue);

public record ParentChildDashboardDto(
    Guid     StudentId,
    Guid     EnrollmentId,
    string   FirstName,
    string   LastName,
    string?  CurrentClass,
    decimal  AttendancePercentage,
    List<ParentChildRecentGrade> RecentGrades,
    int      PendingAssignments,
    int      UpcomingExamCount);

public record ParentChildRecentGrade(
    string  SubjectName,
    string  ExamName,
    decimal Score,
    decimal MaxScore,
    decimal Percentage,
    string  Grade);
