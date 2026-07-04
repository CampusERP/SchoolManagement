namespace Application.Common.Interfaces;

/// <summary>
/// Represents a unit of work that encapsulates a set of operations to be performed as a single transaction.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
