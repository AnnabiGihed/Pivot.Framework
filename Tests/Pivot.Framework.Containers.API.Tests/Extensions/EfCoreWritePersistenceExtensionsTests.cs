using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pivot.Framework.Application.Abstractions;
using Pivot.Framework.Infrastructure.Abstraction.EventStore.Repositories;
using Pivot.Framework.Infrastructure.Abstraction.Outbox.DomainEventPublisher;
using Pivot.Framework.Infrastructure.Abstraction.Outbox.Repositories;
using Pivot.Framework.Infrastructure.Abstraction.Persistence;
using Pivot.Framework.Infrastructure.Abstraction.Transaction;
using Pivot.Framework.Infrastructure.Abstraction.UnitOfWork;
using Pivot.Framework.Infrastructure.Messaging.EntityFrameworkCore.Extensions;
using Pivot.Framework.Infrastructure.Messaging.EntityFrameworkCore.MessageBrokers.RabbitMQ;
using Pivot.Framework.Infrastructure.Persistence.EntityFrameworkCore.Outbox.Publisher;
using Pivot.Framework.Infrastructure.Persistence.EntityFrameworkCore.Services;
using Pivot.Framework.Infrastructure.Persistence.EntityFrameworkCore.Transaction;
using Pivot.Framework.Infrastructure.Persistence.EntityFrameworkCore.UnitOfWork;

namespace Pivot.Framework.Containers.API.Tests.Extensions;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 04-2026
/// Purpose     : Unit tests for <see cref="EfCoreWritePersistenceExtensions"/>.
///              Verifies that the transport-agnostic EF Core write-side services are
///              registered correctly and remain decoupled from RabbitMQ transport setup.
/// </summary>
public class EfCoreWritePersistenceExtensionsTests
{
	#region Test Infrastructure

	public sealed class TestDbContext : DbContext, IPersistenceContext
	{
		public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
		{
		}
	}

	public sealed class TestUnitOfWork : UnitOfWork<TestDbContext>
	{
		public TestUnitOfWork(
			TestDbContext dbContext,
			ICurrentUserProvider currentUserProvider,
			IDomainEventPublisher<TestDbContext> domainEventPublisher)
			: base(dbContext, currentUserProvider, domainEventPublisher)
		{
		}
	}

	private static ServiceCollection CreateServices()
	{
		var services = new ServiceCollection();
		services.AddLogging();
		services.AddDbContext<TestDbContext>(options =>
			options.UseInMemoryDatabase(Guid.NewGuid().ToString()));
		return services;
	}

	#endregion

	#region AddEfCoreWritePersistence Tests

	[Fact]
	public void AddEfCoreWritePersistence_ShouldRegisterTransactionManager()
	{
		var services = CreateServices();

		services.AddEfCoreWritePersistence<TestDbContext, TestUnitOfWork>();

		var descriptor = services.SingleOrDefault(x =>
			x.ServiceType == typeof(ITransactionManager<TestDbContext>));

		descriptor.Should().NotBeNull();
		descriptor!.ImplementationType.Should().Be(typeof(TransactionManager<TestDbContext>));
		descriptor.Lifetime.Should().Be(ServiceLifetime.Scoped);
	}

	[Fact]
	public void AddEfCoreWritePersistence_ShouldRegisterOutboxRepository()
	{
		var services = CreateServices();

		services.AddEfCoreWritePersistence<TestDbContext, TestUnitOfWork>();

		var descriptor = services.SingleOrDefault(x =>
			x.ServiceType == typeof(IOutboxRepository<TestDbContext>));

		descriptor.Should().NotBeNull();
		descriptor!.Lifetime.Should().Be(ServiceLifetime.Scoped);
	}

	[Fact]
	public void AddEfCoreWritePersistence_ShouldRegisterDomainEventPublishers()
	{
		var services = CreateServices();

		services.AddEfCoreWritePersistence<TestDbContext, TestUnitOfWork>();

		services.Should().Contain(x => x.ServiceType == typeof(IDomainEventPublisher<TestDbContext>));
		services.Should().Contain(x => x.ServiceType == typeof(IDomainEventPublisher));
	}

	[Fact]
	public void AddEfCoreWritePersistence_ShouldRegisterCurrentUserInfrastructure()
	{
		var services = CreateServices();

		services.AddEfCoreWritePersistence<TestDbContext, TestUnitOfWork>();

		services.Should().Contain(x => x.ServiceType == typeof(IHttpContextAccessor));
		services.Should().Contain(x =>
			x.ServiceType == typeof(ICurrentUserProvider) &&
			x.ImplementationType == typeof(HttpContextCurrentUserProvider));
	}

	[Fact]
	public void AddEfCoreWritePersistence_ShouldRegisterUnitOfWork()
	{
		var services = CreateServices();

		services.AddEfCoreWritePersistence<TestDbContext, TestUnitOfWork>();

		var descriptor = services.SingleOrDefault(x =>
			x.ServiceType == typeof(IUnitOfWork<TestDbContext>));

		descriptor.Should().NotBeNull();
		descriptor!.ImplementationType.Should().Be(typeof(TestUnitOfWork));
		descriptor.Lifetime.Should().Be(ServiceLifetime.Scoped);
	}

	[Fact]
	public void AddEfCoreWritePersistence_WithEventStore_ShouldRegisterEventStoreRepository()
	{
		var services = CreateServices();

		services.AddEfCoreWritePersistence<TestDbContext, TestUnitOfWork>(includeEventStore: true);

		services.Should().Contain(x => x.ServiceType == typeof(IEventStoreRepository<TestDbContext>));
	}

	[Fact]
	public void AddEfCoreWritePersistence_WithoutEventStore_ShouldNotRegisterEventStoreRepository()
	{
		var services = CreateServices();

		services.AddEfCoreWritePersistence<TestDbContext, TestUnitOfWork>();

		services.Should().NotContain(x => x.ServiceType == typeof(IEventStoreRepository<TestDbContext>));
	}

	[Fact]
	public void AddEfCoreWritePersistence_CalledTwice_ShouldNotDuplicateScopedRegistrations()
	{
		var services = CreateServices();

		services.AddEfCoreWritePersistence<TestDbContext, TestUnitOfWork>(includeEventStore: true);
		services.AddEfCoreWritePersistence<TestDbContext, TestUnitOfWork>(includeEventStore: true);

		services.Count(x => x.ServiceType == typeof(ITransactionManager<TestDbContext>)).Should().Be(1);
		services.Count(x => x.ServiceType == typeof(IOutboxRepository<TestDbContext>)).Should().Be(1);
		services.Count(x => x.ServiceType == typeof(IDomainEventPublisher<TestDbContext>)).Should().Be(1);
		services.Count(x => x.ServiceType == typeof(ICurrentUserProvider)).Should().Be(1);
		services.Count(x => x.ServiceType == typeof(IUnitOfWork<TestDbContext>)).Should().Be(1);
		services.Count(x => x.ServiceType == typeof(IEventStoreRepository<TestDbContext>)).Should().Be(1);
	}

	[Fact]
	public void AddEfCoreWritePersistence_ShouldResolveConcreteUnitOfWorkGraph()
	{
		var services = CreateServices();

		services.AddEfCoreWritePersistence<TestDbContext, TestUnitOfWork>(includeEventStore: true);

		using var provider = services.BuildServiceProvider();
		using var scope = provider.CreateScope();

		var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork<TestDbContext>>();
		var publisher = scope.ServiceProvider.GetRequiredService<IDomainEventPublisher<TestDbContext>>();

		unitOfWork.Should().BeOfType<TestUnitOfWork>();
		publisher.Should().BeOfType<DomainEventPublisher<TestDbContext>>();
	}

	#endregion

	#region RabbitMQ Boundary Tests

	[Fact]
	public void AddRabbitMQPublisher_ShouldNotRegisterOutboxRepository()
	{
		var services = new ServiceCollection();
		var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
		{
			["RabbitMQ:HostName"] = "localhost",
			["RabbitMQ:UserName"] = "guest",
			["RabbitMQ:Password"] = "guest",
			["RabbitMQ:VirtualHost"] = "/",
			["RabbitMQ:ExchangeName"] = "pivot.exchange",
			["RabbitMQ:QueueName"] = "pivot.queue",
			["RabbitMQ:EncryptionKey"] = "12345678901234561234567890123456"
		}).Build();

		services.AddRabbitMQPublisher(configuration);

		services.Should().NotContain(x => x.ServiceType == typeof(IOutboxRepository<>));
	}

	#endregion
}
