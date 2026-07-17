namespace Application.Common.Models;

public record TokenResult(string AccessToken, string RefreshToken, DateTime AccessTokenExpiry);