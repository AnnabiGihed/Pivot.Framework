using Pivot.Framework.Infrastructure.Abstraction.Persistence;

namespace Pivot.Framework.Infrastructure.Abstraction.Outbox.IntegrationEventPublisher;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 04-2026
/// Purpose     : Context-specific variant of <see cref="IIntegrationEventPublisher"/>.
///              Enables safe DI registration when multiple persistence contexts coexist
///              in the same process, while preserving the existing non-generic abstraction
///              for backwards compatibility.
/// </summary>
/// <typeparam name="TContext">
/// The persistence context type used as a DI discriminator for the publisher.
/// </typeparam>
public interface IIntegrationEventPublisher<TContext> : IIntegrationEventPublisher
	where TContext : class, IPersistenceContext
{
}
