using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Pivot.Framework.Containers.API.Middleware;
using Pivot.Framework.Domain.Shared;
using Pivot.Framework.Infrastructure.Abstraction.Outbox.Processor;
using Pivot.Framework.Infrastructure.Abstraction.Persistence;

namespace Pivot.Framework.Containers.API.Tests.Middleware;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Unit tests for <see cref="OutboxProcessingMiddleware{TContext}"/>.
///              Verifies pass-through, 2xx processing, non-2xx skipping,
///              missing processor handling, and exception logging.
/// </summary>
public class OutboxProcessingMiddlewareTests
{
	#region Test Infrastructure
	public class TestDbContext : DbContext, IPersistenceContext
	{
		public TestDbContext() : base(new DbContextOptionsBuilder<TestDbContext>()
			.UseInMemoryDatabase(Guid.NewGuid().ToString()).Options) { }
	}

	private class FakeOutboxProcessor : IOutboxProcessor<TestDbContext>
	{
		public int CallCount { get; private set; }
		public bool ShouldThrow { get; set; }

		public Task<Result> ProcessOutboxMessagesAsync(CancellationToken cancellationToken)
		{
			if (ShouldThrow)
				throw new InvalidOperationException("Processing failed");

			CallCount++;
			return Task.FromResult(Result.Success());
		}
	}

	private static ILogger<OutboxProcessingMiddleware<TestDbContext>> CreateLogger() =>
		NullLoggerFactory.Instance.CreateLogger<OutboxProcessingMiddleware<TestDbContext>>();
	#endregion

	#region Constructor Guard Tests
	/// <summary>
	/// Verifies that null next delegate throws.
	/// </summary>
	[Fact]
	public void Constructor_NullNext_ShouldThrow()
	{
		var act = () => new OutboxProcessingMiddleware<TestDbContext>(null!, CreateLogger());

		act.Should().Throw<ArgumentNullException>();
	}

	/// <summary>
	/// Verifies that null logger throws.
	/// </summary>
	[Fact]
	public void Constructor_NullLogger_ShouldThrow()
	{
		var act = () => new OutboxProcessingMiddleware<TestDbContext>(_ => Task.CompletedTask, null!);

		act.Should().Throw<ArgumentNullException>();
	}
	#endregion

	#region InvokeAsync Tests
	/// <summary>
	/// Verifies that next delegate is always called.
	/// </summary>
	[Fact]
	public async Task InvokeAsync_ShouldCallNextDelegate()
	{
		var nextCalled = false;
		var middleware = new OutboxProcessingMiddleware<TestDbContext>(
			_ => { nextCalled = true; return Task.CompletedTask; }, CreateLogger());

		var context = new DefaultHttpContext();

		await middleware.InvokeAsync(context);

		nextCalled.Should().BeTrue();
	}

	/// <summary>
	/// Verifies that outbox processor is called for 2xx responses.
	/// </summary>
	[Fact]
	public async Task InvokeAsync_SuccessStatus_ShouldProcessOutbox()
	{
		var processor = new FakeOutboxProcessor();

		var services = new ServiceCollection();
		services.AddSingleton<IOutboxProcessor<TestDbContext>>(processor);
		var serviceProvider = services.BuildServiceProvider();

		var middleware = new OutboxProcessingMiddleware<TestDbContext>(
			ctx => { ctx.Response.StatusCode = 200; return Task.CompletedTask; }, CreateLogger());

		var context = new DefaultHttpContext { RequestServices = serviceProvider };

		await middleware.InvokeAsync(context);

		processor.CallCount.Should().Be(1);
	}

	/// <summary>
	/// Verifies that outbox processor is NOT called for non-2xx responses.
	/// </summary>
	[Theory]
	[InlineData(400)]
	[InlineData(404)]
	[InlineData(500)]
	[InlineData(199)]
	[InlineData(301)]
	public async Task InvokeAsync_NonSuccessStatus_ShouldNotProcessOutbox(int statusCode)
	{
		var processor = new FakeOutboxProcessor();

		var services = new ServiceCollection();
		services.AddSingleton<IOutboxProcessor<TestDbContext>>(processor);
		var serviceProvider = services.BuildServiceProvider();

		var middleware = new OutboxProcessingMiddleware<TestDbContext>(
			ctx => { ctx.Response.StatusCode = statusCode; return Task.CompletedTask; }, CreateLogger());

		var context = new DefaultHttpContext { RequestServices = serviceProvider };

		await middleware.InvokeAsync(context);

		processor.CallCount.Should().Be(0);
	}

	/// <summary>
	/// Verifies that missing processor is handled gracefully (no throw).
	/// </summary>
	[Fact]
	public async Task InvokeAsync_NoProcessorRegistered_ShouldNotThrow()
	{
		var services = new ServiceCollection();
		var serviceProvider = services.BuildServiceProvider();

		var middleware = new OutboxProcessingMiddleware<TestDbContext>(
			_ => Task.CompletedTask, CreateLogger());

		var context = new DefaultHttpContext { RequestServices = serviceProvider };

		var act = () => middleware.InvokeAsync(context);

		await act.Should().NotThrowAsync();
	}

	/// <summary>
	/// Verifies that processor exceptions are caught and logged.
	/// </summary>
	[Fact]
	public async Task InvokeAsync_ProcessorThrows_ShouldCatchAndLog()
	{
		var processor = new FakeOutboxProcessor { ShouldThrow = true };

		var services = new ServiceCollection();
		services.AddSingleton<IOutboxProcessor<TestDbContext>>(processor);
		var serviceProvider = services.BuildServiceProvider();

		var middleware = new OutboxProcessingMiddleware<TestDbContext>(
			_ => Task.CompletedTask, CreateLogger());

		var context = new DefaultHttpContext { RequestServices = serviceProvider };

		var act = () => middleware.InvokeAsync(context);

		await act.Should().NotThrowAsync();
	}

	/// <summary>
	/// Verifies that 201 Created status triggers processing.
	/// </summary>
	[Fact]
	public async Task InvokeAsync_Status201_ShouldProcessOutbox()
	{
		var processor = new FakeOutboxProcessor();

		var services = new ServiceCollection();
		services.AddSingleton<IOutboxProcessor<TestDbContext>>(processor);
		var serviceProvider = services.BuildServiceProvider();

		var middleware = new OutboxProcessingMiddleware<TestDbContext>(
			ctx => { ctx.Response.StatusCode = 201; return Task.CompletedTask; }, CreateLogger());

		var context = new DefaultHttpContext { RequestServices = serviceProvider };

		await middleware.InvokeAsync(context);

		processor.CallCount.Should().Be(1);
	}
	#endregion
}
