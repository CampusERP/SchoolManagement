namespace Application.Features.People.Queries.GetTeachers;

public record TeacherListDto(Guid Id, string EmployeeCode, string FirstName,
    string LastName, string? Email, string EmploymentStatus, int AssignedClassesCount);
