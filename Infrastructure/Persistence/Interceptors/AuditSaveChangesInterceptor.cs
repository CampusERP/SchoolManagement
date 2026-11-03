using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Application.Common.Interfaces.Services;

namespace Infrastructure.Persistence.Interceptors;

/// <summary>
/// Automatically updates audit and soft-delete fields on every tracked entity that exposes them.
/// </summary>
public class AuditSaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUserService _currentUserService;

    public AuditSaveChangesInterceptor(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData, InterceptionResult<int> result)
    {
        UpdateAuditFields(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        UpdateAuditFields(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void UpdateAuditFields(DbContext? context)
    {
        if (context is null) return;

        var now = DateTime.UtcNow;
        var userId = _currentUserService.UserId;

        foreach (var entry in context.ChangeTracker.Entries())
        {
            var hasCreatedAt = HasProperty(entry, "CreatedAtUtc");
            var hasCreatedBy = HasProperty(entry, "CreatedBy");
            var hasModifiedAt = HasProperty(entry, "ModifiedAtUtc");
            var hasModifiedBy = HasProperty(entry, "ModifiedBy");
            var hasIsDeleted = HasProperty(entry, "IsDeleted");
            var hasDeletedAt = HasProperty(entry, "DeletedAtUtc");
            var hasDeletedBy = HasProperty(entry, "DeletedBy");

            switch (entry.State)
            {
                case EntityState.Added:
                    if (hasCreatedAt) SetProperty(entry, "CreatedAtUtc", now);
                    if (hasCreatedBy) SetProperty(entry, "CreatedBy", userId);
                    break;

                case EntityState.Modified:
                    if (hasModifiedAt) SetProperty(entry, "ModifiedAtUtc", now);
                    if (hasModifiedBy) SetProperty(entry, "ModifiedBy", userId);
                    break;

                case EntityState.Deleted:
                    if (hasIsDeleted)
                    {
                        // Convert hard delete -> soft delete when the entity supports it.
                        entry.State = EntityState.Modified;
                        SetProperty(entry, "IsDeleted", true);
                        if (hasDeletedAt) SetProperty(entry, "DeletedAtUtc", now);
                        if (hasDeletedBy) SetProperty(entry, "DeletedBy", userId);
                        if (hasModifiedAt) SetProperty(entry, "ModifiedAtUtc", now);
                        if (hasModifiedBy) SetProperty(entry, "ModifiedBy", userId);
                    }
                    break;
            }
        }
    }

    private static bool HasProperty(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry, string name)
        => entry.Metadata.FindProperty(name) is not null;

    private static void SetProperty(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry, string name, object? value)
        => entry.Property(name).CurrentValue = value;
}
