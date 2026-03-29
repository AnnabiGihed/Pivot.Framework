using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Pivot.Framework.Application.Abstractions.Sagas;
using Pivot.Framework.Infrastructure.Abstraction.Persistence;
using Pivot.Framework.Infrastructure.Abstraction.Sagas.Repositories;
using Pivot.Framework.Infrastructure.Persistence.EntityFrameworkCore.Sagas;
using Pivot.Framework.Infrastructure.Persistence.EntityFrameworkCore.Sagas.Repositories;

namespace Pivot.Framework.Infrastructure.Messaging.EntityFrameworkCore.Sagas.Extensions;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : DI registration extensions for the saga orchestration pattern.
///              Registers the saga repository and orchestrator for the given persistence context.
///
///              Usage:
///              <code>
///              services.AddSagaSupport&lt;MyDbContext&gt;();
///              </code>
///
///              Applications define sagas by implementing <see cref="ISagaDefinition{TSagaData}"/>
///              and <see cref="ISagaStep{TSagaData}"/>, then invoke the orchestrator:
///              <code>
///              var orchestrator = serviceProvider.GetRequiredService&lt;ISagaOrchestrator&gt;();
///              var result = await orchestrator.StartAsync(sagaDefinition, sagaData, correlationId);
///              </code>
/// </summary>
public static class SagaExtensions
{
	/// <summary>
	/// Registers the saga orchestration infrastructure for the given <typeparamref name="TContext"/>.
	/// This includes:
	/// <list type="bullet">
	///   <item><see cref="ISagaRepository{TContext}"/> → <see cref="SagaRepository{TContext}"/></item>
	///   <item><see cref="ISagaOrchestrator"/> → <see cref="SagaOrchestrator{TContext}"/></item>
	/// </list>
	/// </summary>
	/// <typeparam name="TContext">The EF Core DbContext that implements <see cref="IPersistenceContext"/>.</typeparam>
	/// <param name="services">The service collection to register into.</param>
	/// <returns>The same <paramref name="services"/> instance for chaining.</returns>
	public static IServiceCollection AddSagaSupport<TContext>(this IServiceCollection services)
		where TContext : DbContext, IPersistenceContext
	{
		services.TryAddScoped(typeof(ISagaRepository<>), typeof(SagaRepository<>));
		services.TryAddScoped<ISagaOrchestrator, SagaOrchestrator<TContext>>();

		return services;
	}
}
