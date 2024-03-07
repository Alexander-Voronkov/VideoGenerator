using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using VideoGenerator.Infrastructure.Entities;

namespace VideoGenerator.Infrastructure.Interceptors;

public class AuditableEntityInterceptor : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        AddStolenDate(eventData);
        AddPublishedDate(eventData);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default(CancellationToken))
    {
        AddStolenDate(eventData);
        AddPublishedDate(eventData);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void AddStolenDate(DbContextEventData eventData)
    {
        foreach (var item in eventData.Context.ChangeTracker.Entries<QueueMessage>())
        {
            if (item.State is EntityState.Added)
            {
                item.Entity.StolenAt = DateTime.UtcNow;
            }
        }
    }

    private void AddPublishedDate(DbContextEventData eventData)
    {
        foreach (var item in eventData.Context.ChangeTracker.Entries<PublishedMessage>())
        {
            if (item.State is EntityState.Added)
            {
                item.Entity.PublishedAt = DateTime.UtcNow;
            }
        }
    }
}