using Domain.Enums;

namespace Application.Features.Portal.Queries.GetStudentDashboard;

public record StudentDashboardDto(
    Guid    StudentId,
    string  StudentCode,
    string  FirstName,
    string  LastName,
    string  AcademicYearName,
    string  ClassRoomName,
    string  GradeLevelName,
    Guid    CurrentEnrollmentId,
    List<StudentDashboardScheduleSlot> TodaySchedule,
    List<StudentDashboardAssignment>  UpcomingAssignments,
    int     TotalAttendanceDays,
    int     PresentDays,
    decimal AttendancePercentage,
    List<StudentDashboardExamResult>  RecentGrades,
    int     UnreadNotificationCount,
    decimal? OverallPercentage,
    string?  OverallGrade);

public record StudentDashboardScheduleSlot(
    string  SubjectName,
    string  TeacherName,
    string  RoomName,
    TimeSpan StartTime,
    TimeSpan EndTime);

public record StudentDashboardAssignment(
    Guid    AssignmentId,
    string  Title,
    string  SubjectName,
    DateTime DueDate,
    decimal? MaxScore,
    SubmissionStatus? SubmissionStatus,
    decimal? Grade);

public record StudentDashboardExamResult(
    string  SubjectName,
    string  ExamName,
    decimal Score,
    decimal MaxScore,
    decimal Percentage,
    string  Grade);
