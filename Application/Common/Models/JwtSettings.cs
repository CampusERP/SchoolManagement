namespace Application.Common.Models;

public class JwtSettings
{
    public string Secret { get; set; } = default!;
    public string Issuer { get; set; } = default!;
    public string Audience { get; set; } = default!;
    public int AccessTokenLifetimeMinutes { get; set; } = 15;
    public int RefreshTokenLifetimeDays { get; set; } = 30;
}