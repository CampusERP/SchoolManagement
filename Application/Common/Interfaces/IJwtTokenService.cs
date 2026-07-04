namespace Application.Common.Interfaces;

public record TokenResult(string AccessToken, string RefreshToken, DateTime AccessTokenExpiry);

public interface IJwtTokenService
{
    /// <summary>
    /// Issues a new access token and refresh token pair for the given user.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="email"></param>
    /// <param name="schoolId"></param>
    /// <param name="permissions"></param>
    /// <param name="isPlatformAdmin"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task<TokenResult> IssueTokensAsync(
        Guid userId,
        string email,
        Guid? schoolId,
        IEnumerable<string> permissions,
        bool isPlatformAdmin,
        CancellationToken ct = default);

    /// <summary>
    /// Refreshes the access token using the provided refresh token.
    /// </summary>
    /// <param name="refreshToken"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task<TokenResult> RefreshAsync(string refreshToken, CancellationToken ct = default);
}
