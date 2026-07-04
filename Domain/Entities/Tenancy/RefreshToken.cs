using Domain.Common;
using Domain.Exceptions;

namespace Domain.Entities.Tenancy;

/// <summary>
/// Represents a refresh token used for authentication and authorization purposes.
/// </summary>
public class RefreshToken : AuditableEntity, IAggregateRoot
{
    public Guid ApplicationUserId { get; private set; }
    public string Token { get; private set; } = default!;
    public DateTime ExpiresAtUtc { get; private set; }
    public DateTime? RevokedAtUtc { get; private set; }
    public string? ReplacedByToken { get; private set; }

    public bool IsActive => RevokedAtUtc is null && ExpiresAtUtc > DateTime.UtcNow;

    private RefreshToken() { } // EF Core

    private RefreshToken(Guid id, Guid applicationUserId, string token, DateTime expiresAtUtc) : base(id)
    {
        ApplicationUserId = applicationUserId;
        Token = token;
        ExpiresAtUtc = expiresAtUtc;
    }

    public static RefreshToken Issue(Guid applicationUserId, string token, TimeSpan lifetime)
    {
        return new RefreshToken(Guid.NewGuid(), applicationUserId, token, DateTime.UtcNow.Add(lifetime));
    }

    public void Revoke(string? replacedByToken = null)
    {
        if (RevokedAtUtc is not null)
            throw new DomainException("Refresh token is already revoked.");

        RevokedAtUtc = DateTime.UtcNow;
        ReplacedByToken = replacedByToken;
    }
}
