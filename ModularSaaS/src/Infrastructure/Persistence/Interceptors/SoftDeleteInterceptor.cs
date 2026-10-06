using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using ModularSaaS.Application.Shared.Abstractions;
using ModularSaaS.Domain.Shared;

namespace ModularSaaS.Infrastructure.Persistence.Interceptors;

internal sealed class SoftDeleteInterceptor(IClock clock, ICurrentUser currentUser) : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        ApplySoftDelete(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        ApplySoftDelete(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    internal void ApplySoftDelete(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        var now = clock.UtcNow;
        var userId = currentUser.UserId;

        foreach (var entry in context.ChangeTracker.Entries<ISoftDeletable>())
        {
            if (entry.State == EntityState.Deleted)
            {
                entry.State = EntityState.Modified;
                entry.Entity.IsDeleted = true;
                entry.Entity.DeletedAtUtc = now;
                entry.Entity.DeletedBy = userId;
            }
            else if (entry.State == EntityState.Modified && entry.Entity.IsDeleted && entry.Entity.DeletedAtUtc is null)
            {
                entry.Entity.DeletedAtUtc = now;
                entry.Entity.DeletedBy = userId;
            }
        }
    }
}
