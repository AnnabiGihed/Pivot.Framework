using Microsoft.EntityFrameworkCore;
using Pivot.Framework.Infrastructure.Abstraction.Persistence;
using Pivot.Framework.Infrastructure.Persistence.EntityFrameworkCore.Configuration;

namespace Pivot.Framework.Infrastructure.Persistence.EntityFrameworkCore.PersistenceContext;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 01-2026
/// Purpose     : Base DbContext that applies shared EF Core conventions and mappings
///              (domain primitives, owned types, common configurations) for all derived contexts.
/// </summary>
public abstract class PivotDbContextBase : DbContext, IPersistenceContext
{
	#region Constructors
	/// <summary>
	/// Initialises a new <see cref="PivotDbContextBase"/> with the provided EF Core options.
	/// </summary>
	/// <param name="options">The EF Core database context options.</param>
	protected PivotDbContextBase(DbContextOptions options)
		: base(options)
	{
	}
	#endregion

	#region Protected Methods
	/// <summary>
	/// Applies common model configuration for all derived contexts.
	/// Derived contexts must call <c>base.OnModelCreating(modelBuilder)</c>.
	/// </summary>
	/// <param name="modelBuilder">The EF Core model builder.</param>
	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);

		modelBuilder.ApplyDomainPrimitives();
	}
	#endregion
}
