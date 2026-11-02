namespace Application.Features.Schools.Queries.GetSchoolById;

public record CampusDto(Guid Id, string Name, string Address);
public record SchoolAdminDto(Guid UserId, string FirstName, string LastName, string? Email);

public record SchoolDetailDto(Guid Id, string Name, string SubdomainCode,
    string Status, string? Address, string? Phone, string? Email,
    List<CampusDto> Campuses, List<SchoolAdminDto> Admins);
