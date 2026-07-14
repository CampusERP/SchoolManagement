using Domain.Common;
using Domain.Exceptions;

namespace Domain.Entities.Tenancy;

/// <summary>
/// Represents a refresh token used for authentication and authorization purposes.
/// </summary>
public class RefreshToken : AuditableEntity, IAggregateRoot
{
    public Guid ApplicationUserId { get; private set; }
    public Guid? SchoolId { get; private set; }
    public string Token { get; private set; } = default!;
    public DateTime ExpiresAtUtc { get; private set; }
    public DateTime? RevokedAtUtc { get; private set; }
    public string? ReplacedByToken { get; private set; }

    public bool IsActive => RevokedAtUtc is null && ExpiresAtUtc > DateTime.UtcNow;

    private RefreshToken() { }

    private RefreshToken(Guid id, Guid applicationUserId, Guid? schoolId, string token, DateTime expiresAtUtc)
        : base(id)
    {
        ApplicationUserId = applicationUserId;
        SchoolId = schoolId;
        Token = token;
        ExpiresAtUtc = expiresAtUtc;
    }

    public static RefreshToken Issue(Guid applicationUserId, Guid? schoolId, string token, TimeSpan lifetime)
        => new(Guid.NewGuid(), applicationUserId, schoolId, token, DateTime.UtcNow.Add(lifetime));

    public void Revoke(string? replacedByToken = null)
    {
        if (RevokedAtUtc is not null)
            throw new DomainException("Refresh token is already revoked.");

        RevokedAtUtc = DateTime.UtcNow;
        ReplacedByToken = replacedByToken;
    }
}
