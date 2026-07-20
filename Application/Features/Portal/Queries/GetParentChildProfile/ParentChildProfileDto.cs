namespace Application.Features.Portal.Queries.GetParentChildProfile;

public record ParentChildProfileDto(
    Guid     StudentId,
    string   StudentCode,
    string   FirstName,
    string   LastName,
    DateTime DateOfBirth,
    string?  CurrentClass,
    string?  GradeLevelName,
    string?  AcademicYearName,
    decimal  AttendancePercentage,
    int      TotalAssignments,
    int      GradedAssignments,
    decimal? LatestOverallPercentage,
    string?  LatestOverallGrade);
