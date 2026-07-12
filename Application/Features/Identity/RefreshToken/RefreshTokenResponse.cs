namespace Application.Features.Identity.RefreshToken;

public record RefreshTokenResponse(string AccessToken, string RefreshToken, DateTime AccessTokenExpiry);
