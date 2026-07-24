namespace Application.Features.People.Queries.GetTeacherById;

public record TeacherDetailDto(Guid Id, string EmployeeCode, string FirstName,
    string LastName, string? Email, string EmploymentStatus,
    List<TeachingAssignmentDetailDto> Assignments);
