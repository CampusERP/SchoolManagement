namespace Application.Features.Schools.Queries.GetAllSchools;

public record SchoolListDto(Guid Id, string Name, string SubdomainCode,
    string Status, int TotalStudents, int TotalTeachers, DateTime CreatedAtUtc);
