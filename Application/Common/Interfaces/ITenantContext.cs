namespace Application.Common.Interfaces;

/// <summary>
/// Represents the context of the current tenant (school) in a multi-tenant application.
/// </summary>
public interface ITenantContext
{
    Guid? SchoolId { get; }
    bool IsPlatformAdmin { get; }
}
