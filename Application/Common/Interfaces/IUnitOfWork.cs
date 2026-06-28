namespace Application.Common.Interfaces;

/// <summary>
/// Thin abstraction over DbContext.SaveChangesAsync so Application handlers
/// don't depend on EF Core directly.
/// it's just a wrapper, not a custom transaction manager.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
