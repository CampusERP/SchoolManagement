namespace Application.Features.Schools.Queries.GetSchoolById;

public record CampusDto(Guid Id, string Name, string Address);

public record SchoolDetailDto(Guid Id, string Name, string SubdomainCode,
    string Status, List<CampusDto> Campuses);
