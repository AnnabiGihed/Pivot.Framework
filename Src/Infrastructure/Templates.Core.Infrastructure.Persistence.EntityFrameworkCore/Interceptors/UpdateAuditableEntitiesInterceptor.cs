using Microsoft.EntityFrameworkCore;
using Templates.Core.Domain.Primitives;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Templates.Core.Infrastructure.Persistence.EntityFrameworkCore.Interceptors;

public sealed class UpdateAuditableEntitiesInterceptor
	: SaveChangesInterceptor
{
	public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
		DbContextEventData eventData,
		InterceptionResult<int> result,
		CancellationToken cancellationToken = default)
	{
		DbContext? dbContext = eventData.Context;

		if (dbContext is null)
		{
			return base.SavingChangesAsync(
				eventData,
				result,
				cancellationToken);
		}

		IEnumerable<EntityEntry<IAuditableEntity>> entries =
			dbContext
				.ChangeTracker
				.Entries<IAuditableEntity>();

		foreach (EntityEntry<IAuditableEntity> entityEntry in entries)
		{
			if (entityEntry.State == EntityState.Added)
			{
				entityEntry.Property(a => a.Audit.CreatedOnUtc).CurrentValue = DateTime.UtcNow;
			}

			if (entityEntry.State == EntityState.Modified)
			{
				entityEntry.Property(a => a.Audit.ModifiedOnUtc).CurrentValue = DateTime.UtcNow;
			}
		}

		return base.SavingChangesAsync(
			eventData,
			result,
			cancellationToken);
	}
}
