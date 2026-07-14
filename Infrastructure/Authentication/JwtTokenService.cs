using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Application.Common.Models;
using Domain.Entities.Tenancy;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Infrastructure.Authentication;

public class JwtTokenService : IJwtTokenService
{
    private readonly JwtSettings _settings;
    private readonly PlatformDbContext _platformDb;
    private readonly IUserSchoolMembershipRepository _memberships;
    private readonly IIdentityService _identityService;
    private readonly IPermissionProvider _permissions;

    public JwtTokenService(
        IOptions<JwtSettings> settings,
        PlatformDbContext platformDb,
        IUserSchoolMembershipRepository memberships,
        IIdentityService identityService,
        IPermissionProvider permissions)
    {
        _settings = settings.Value;
        _platformDb = platformDb;
        _memberships = memberships;
        _identityService = identityService;
        _permissions = permissions;
    }

    public async Task<TokenResult> IssueTokensAsync(
        Guid userId,
        string email,
        Guid? schoolId,
        IEnumerable<string> permissions,
        bool isPlatformAdmin,
        CancellationToken ct = default)
    {
        var permissionList = permissions.ToList();
        var (accessToken, expiry) = GenerateAccessToken(userId, email, schoolId, permissionList, isPlatformAdmin);
        var refreshTokenValue = GenerateRefreshTokenValue();

        var refreshToken = RefreshToken.Issue(
            userId, schoolId, refreshTokenValue, TimeSpan.FromDays(_settings.RefreshTokenLifetimeDays));

        _platformDb.RefreshTokens.Add(refreshToken);
        await _platformDb.SaveChangesAsync(ct);

        return new TokenResult(accessToken, refreshTokenValue, expiry);
    }

    public async Task<TokenResult> RefreshAsync(string refreshToken, CancellationToken ct = default)
    {
        var existing = await _platformDb.RefreshTokens
            .FirstOrDefaultAsync(t => t.Token == refreshToken, ct);

        if (existing is null || !existing.IsActive)
            throw new UnauthorizedAccessException("Invalid or expired refresh token.");

        var user = await _identityService.GetByIdAsync(existing.ApplicationUserId, ct);
        if (user is null)
            throw new UnauthorizedAccessException("User no longer exists.");

        var (role, permissionList) = await ResolveRoleAndPermissionsAsync(user, ct);

        var (newAccessToken, expiry) = GenerateAccessToken(
            user.Id, user.Email, existing.SchoolId, permissionList, user.IsPlatformAdmin);

        var newRefreshTokenValue = GenerateRefreshTokenValue();

        existing.Revoke(newRefreshTokenValue);
        var newToken = RefreshToken.Issue(
            user.Id, existing.SchoolId, newRefreshTokenValue, TimeSpan.FromDays(_settings.RefreshTokenLifetimeDays));

        _platformDb.RefreshTokens.Add(newToken);
        await _platformDb.SaveChangesAsync(ct);

        return new TokenResult(newAccessToken, newRefreshTokenValue, expiry);
    }

    public async Task<TokenResult> SwitchSchoolAsync(
        string refreshToken, Guid requestedSchoolId, CancellationToken ct = default)
    {
        var existing = await _platformDb.RefreshTokens
            .FirstOrDefaultAsync(t => t.Token == refreshToken, ct);

        if (existing is null || !existing.IsActive)
            throw new UnauthorizedAccessException("Invalid or expired refresh token.");

        var user = await _identityService.GetByIdAsync(existing.ApplicationUserId, ct);
        if (user is null || user.IsPlatformAdmin)
            throw new UnauthorizedAccessException("School switching is not applicable for this account.");

        var membership = await _memberships.GetAsync(user.Id, requestedSchoolId, ct);
        if (membership is null || !membership.IsActive)
            throw new UnauthorizedAccessException("No active membership in the requested school.");

        var (role, permissionList) = await ResolveRoleAndPermissionsAsync(user, ct);

        var (newAccessToken, expiry) = GenerateAccessToken(
            user.Id, user.Email, requestedSchoolId, permissionList, isPlatformAdmin: false);

        var newRefreshTokenValue = GenerateRefreshTokenValue();

        existing.Revoke(newRefreshTokenValue);
        var newToken = RefreshToken.Issue(
            user.Id, requestedSchoolId, newRefreshTokenValue, TimeSpan.FromDays(_settings.RefreshTokenLifetimeDays));

        _platformDb.RefreshTokens.Add(newToken);
        await _platformDb.SaveChangesAsync(ct);

        return new TokenResult(newAccessToken, newRefreshTokenValue, expiry);
    }

    private async Task<(string role, List<string> permissions)> ResolveRoleAndPermissionsAsync(
        AuthenticatedUser user, CancellationToken ct)
    {
        if (user.IsPlatformAdmin)
            return ("SuperAdmin", _permissions.GetPlatformAdminPermissions());

        var roles = await _identityService.GetRolesAsync(user.Id, ct);
        var role = roles.FirstOrDefault() ?? "Unknown";
        return (role, _permissions.GetPermissionsForRole(role));
    }

    private (string token, DateTime expiry) GenerateAccessToken(
        Guid userId, string email, Guid? schoolId, IEnumerable<string> permissions, bool isPlatformAdmin)
    {
        var expiry = DateTime.UtcNow.AddMinutes(_settings.AccessTokenLifetimeMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Email, email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("is_platform_admin", isPlatformAdmin.ToString())
        };

        if (schoolId.HasValue)
            claims.Add(new Claim("school_id", schoolId.Value.ToString()));

        claims.AddRange(permissions.Select(p => new Claim("permission", p)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: expiry,
            signingCredentials: creds);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiry);
    }

    private static string GenerateRefreshTokenValue()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
    }
}