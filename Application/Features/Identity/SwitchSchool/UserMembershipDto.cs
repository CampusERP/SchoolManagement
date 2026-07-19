namespace Application.Features.Identity.SwitchSchool;

public record UserMembershipDto(
    Guid SchoolId, string SchoolName, string SubdomainCode,
    string Role, bool IsActive, bool IsCurrent);
