namespace Application.Features.Portal.Queries.GetTeacherDashboard;

public record TeacherDashboardDto(
    Guid   TeacherId,
    Guid?  CurrentTermId,
    int    TotalClasses,
    int    TotalStudents,
    int    TodayLessons,
    int    PendingAttendance,
    int    PendingAssignments,
    List<TeacherDashboardScheduleSlot> TodaySchedule,
    List<TeacherDashboardClass> MyClasses,
    List<TeacherDashboardAnnouncement> Announcements);

public record TeacherDashboardScheduleSlot(
    Guid   ClassScheduleId,
    Guid   TeachingAssignmentId,
    string ClassName,
    string Subject,
    string TeacherName,
    string Room,
    TimeSpan StartTime,
    TimeSpan EndTime,
    string Status);

public record TeacherDashboardClass(
    Guid   TeachingAssignmentId,
    Guid   ClassRoomId,
    string Name,
    string Subject,
    string TeacherName,
    int    StudentCount,
    string GradeLevel);

public record TeacherDashboardAnnouncement(
    Guid   Id,
    string Title,
    string Content,
    string Author,
    DateTime CreatedAtUtc);
