namespace Application.Common.Interfaces;

/// <summary>
/// Represents a request that is scoped to a specific tenant (school) in a multi-tenant application.
/// </summary>
public interface ITenantScopedRequest
{
    Guid SchoolId { get; }
}
