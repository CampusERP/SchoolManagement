using Application.Common.Models;

namespace Application.Common.Interfaces.Services;

public interface IJwtTokenService
{
    Task<TokenResult> IssueTokensAsync(
        Guid userId,
        string email,
        Guid? schoolId,
        IEnumerable<string> permissions,
        bool isPlatformAdmin,
        CancellationToken ct = default);
    Task<TokenResult> RefreshAsync(string refreshToken, CancellationToken ct = default);
    Task<TokenResult> SwitchSchoolAsync(string refreshToken, Guid requestedSchoolId, CancellationToken ct = default);
}