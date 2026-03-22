using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Pivot.Framework.Application.Abstractions.ReadModels;
using Pivot.Framework.Infrastructure.Abstraction.Persistence;
using Pivot.Framework.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

namespace Pivot.Framework.Infrastructure.Persistence.EntityFrameworkCore.Extensions;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : DI registration extensions for EF Core read model infrastructure.
///              Registers <see cref="EfCoreReadModelRepository{TReadModel,TId}"/> and
///              <see cref="EfCoreReadModelStore{TReadModel,TId}"/> as the implementations
///              for <see cref="IReadModelRepository{TReadModel,TId}"/> and
///              <see cref="IReadModelStore{TReadModel,TId}"/> respectively.
///
///              Usage:
///              <code>
///              services.AddEfCoreReadModelStore&lt;AppDbContext&gt;();
///              </code>
/// </summary>
public static class EfCoreReadModelExtensions
{
	/// <summary>
	/// Registers EF Core implementations of <see cref="IReadModelRepository{TReadModel,TId}"/>
	/// and <see cref="IReadModelStore{TReadModel,TId}"/> using the specified <typeparamref name="TContext"/>.
	/// </summary>
	/// <typeparam name="TContext">The EF Core <see cref="DbContext"/> type that contains read model DbSets.</typeparam>
	/// <param name="services">The service collection to register into.</param>
	/// <returns>The same <paramref name="services"/> instance for chaining.</returns>
	public static IServiceCollection AddEfCoreReadModelStore<TContext>(
		this IServiceCollection services)
		where TContext : DbContext, IPersistenceContext
	{
		// Register DbContext as a scoped service that resolves the specific TContext,
		// so that EfCoreReadModelRepository/Store (which depend on DbContext) resolve
		// the correct context type supplied by the consumer.
		services.AddScoped<DbContext>(sp => sp.GetRequiredService<TContext>());

		services.AddScoped(typeof(IReadModelRepository<,>), typeof(EfCoreReadModelRepository<,>));
		services.AddScoped(typeof(IReadModelStore<,>), typeof(EfCoreReadModelStore<,>));

		return services;
	}
}
