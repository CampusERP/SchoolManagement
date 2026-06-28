namespace Application.Common.Interfaces;

/// <summary>
/// The single most important interface in the codebase. Implemented in
/// Infrastructure by reading JWT claims off the current HTTP request.
/// ApplicationDbContext's global query filter depends on this for every
/// tenant-scoped table.
/// </summary>
public interface ITenantContext
{
    Guid? SchoolId { get; }
    bool IsPlatformAdmin { get; }
}
