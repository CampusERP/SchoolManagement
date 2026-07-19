namespace Application.Features.Identity.SwitchSchool;

public record SwitchSchoolResponse(
    string AccessToken, string RefreshToken, DateTime AccessTokenExpiry,
    Guid SchoolId, string Role, string SchoolName);
