namespace Application.Features.Identity.Login;

public record LoginResponse(
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiry,
    Guid UserId,
    string Email,
    Guid? SchoolId,
    string Role);