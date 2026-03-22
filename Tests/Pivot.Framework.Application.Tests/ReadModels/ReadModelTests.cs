using FluentAssertions;
using Pivot.Framework.Application.Abstractions.Messaging.Events;
using Pivot.Framework.Application.Abstractions.ReadModels;
using Pivot.Framework.Domain.DomainEvents;
using Pivot.Framework.Domain.Primitives;
using Pivot.Framework.Domain.Shared;

namespace Pivot.Framework.Application.Tests.ReadModels;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Unit tests for the read model abstractions:
///              - <see cref="ReadModel{TId}"/> with various key types
///              - <see cref="ReadModelSpecification{TReadModel}"/> criteria, ordering, paging
///              - <see cref="ProjectionHandler{TEvent}"/> MediatR integration and contract
///              - <see cref="IReadModel{TId}"/> interface compliance
/// </summary>
public class ReadModelTests
{
	#region Test Infrastructure

	private sealed class GuidReadModel : ReadModel<Guid>
	{
		public string Name { get; init; } = string.Empty;
		public GuidReadModel(Guid id) : base(id) { }
		public GuidReadModel() { }
	}

	private sealed class IntReadModel : ReadModel<int>
	{
		public string Name { get; init; } = string.Empty;
		public IntReadModel(int id) : base(id) { }
		public IntReadModel() { }
	}

	private sealed class StringReadModel : ReadModel<string>
	{
		public string Name { get; init; } = string.Empty;
		public StringReadModel(string id) : base(id) { }
		public StringReadModel() { }
	}

	private sealed class TestSpecification : ReadModelSpecification<GuidReadModel>
	{
		public TestSpecification() : base() { }

		public TestSpecification(System.Linq.Expressions.Expression<Func<GuidReadModel, bool>> criteria)
			: base(criteria) { }

		public void SetPaging(int skip, int take) => ApplyPaging(skip, take);

		public void SetOrderBy(System.Linq.Expressions.Expression<Func<GuidReadModel, object>> expr)
			=> AddOrderBy(expr);

		public void SetOrderByDescending(System.Linq.Expressions.Expression<Func<GuidReadModel, object>> expr)
			=> AddOrderByDescending(expr);
	}

	private sealed record TestProjectionEvent(string Data) : DomainEvent;

	private sealed class TestProjection : ProjectionHandler<TestProjectionEvent>
	{
		public TestProjectionEvent? ReceivedEvent { get; private set; }
		public int CallCount { get; private set; }

		public override Task ProjectAsync(TestProjectionEvent domainEvent, CancellationToken ct)
		{
			ReceivedEvent = domainEvent;
			CallCount++;
			return Task.CompletedTask;
		}
	}

	#endregion

	#region ReadModel<TId> — No Constraint on TId

	/// <summary>
	/// Verifies that a ReadModel with Guid ID assigns the ID correctly.
	/// </summary>
	[Fact]
	public void ReadModel_WithGuidId_ShouldAssignId()
	{
		var id = Guid.NewGuid();
		var model = new GuidReadModel(id) { Name = "Test" };

		model.Id.Should().Be(id);
		model.Name.Should().Be("Test");
	}

	/// <summary>
	/// Verifies that a ReadModel with int ID assigns the ID correctly.
	/// </summary>
	[Fact]
	public void ReadModel_WithIntId_ShouldAssignId()
	{
		var model = new IntReadModel(42) { Name = "Test" };

		model.Id.Should().Be(42);
		model.Name.Should().Be("Test");
	}

	/// <summary>
	/// Verifies that a ReadModel with string ID assigns the ID correctly.
	/// </summary>
	[Fact]
	public void ReadModel_WithStringId_ShouldAssignId()
	{
		var model = new StringReadModel("order-123") { Name = "Test" };

		model.Id.Should().Be("order-123");
		model.Name.Should().Be("Test");
	}

	/// <summary>
	/// Verifies that the parameterless constructor sets the default ID value.
	/// </summary>
	[Fact]
	public void ReadModel_ParameterlessConstructor_ShouldSetDefaultId()
	{
		var model = new GuidReadModel();
		model.Id.Should().Be(Guid.Empty);
	}

	/// <summary>
	/// Verifies that constructing a ReadModel with null ID throws.
	/// </summary>
	[Fact]
	public void ReadModel_NullId_ShouldThrow()
	{
		var act = () => new StringReadModel(null!);
		act.Should().Throw<ArgumentNullException>();
	}

	#endregion

	#region IReadModel<TId> — Interface Compliance

	/// <summary>
	/// Verifies that ReadModel with Guid ID implements IReadModel.
	/// </summary>
	[Fact]
	public void ReadModel_ShouldImplementIReadModel()
	{
		var model = new GuidReadModel(Guid.NewGuid());

		model.Should().BeAssignableTo<IReadModel<Guid>>();
		((IReadModel<Guid>)model).Id.Should().Be(model.Id);
	}

	/// <summary>
	/// Verifies that ReadModel with int ID implements IReadModel.
	/// </summary>
	[Fact]
	public void ReadModel_IntId_ShouldImplementIReadModel()
	{
		var model = new IntReadModel(42);

		model.Should().BeAssignableTo<IReadModel<int>>();
		((IReadModel<int>)model).Id.Should().Be(42);
	}

	#endregion

	#region ReadModelSpecification — Criteria

	/// <summary>
	/// Verifies that a specification with no criteria has null Criteria.
	/// </summary>
	[Fact]
	public void Specification_WithNoCriteria_ShouldHaveNullCriteria()
	{
		var spec = new TestSpecification();
		spec.Criteria.Should().BeNull();
	}

	/// <summary>
	/// Verifies that a specification with criteria stores it.
	/// </summary>
	[Fact]
	public void Specification_WithCriteria_ShouldStoreCriteria()
	{
		var spec = new TestSpecification(x => x.Name == "Test");
		spec.Criteria.Should().NotBeNull();
	}

	#endregion

	#region ReadModelSpecification — Paging

	/// <summary>
	/// Verifies that ApplyPaging sets Skip and Take correctly.
	/// </summary>
	[Fact]
	public void Specification_ApplyPaging_ShouldSetSkipAndTake()
	{
		var spec = new TestSpecification();
		spec.SetPaging(10, 25);

		spec.Skip.Should().Be(10);
		spec.Take.Should().Be(25);
	}

	/// <summary>
	/// Verifies that ApplyPaging with negative skip throws.
	/// </summary>
	[Fact]
	public void Specification_ApplyPaging_NegativeSkip_ShouldThrow()
	{
		var spec = new TestSpecification();
		var act = () => spec.SetPaging(-1, 10);
		act.Should().Throw<ArgumentOutOfRangeException>();
	}

	/// <summary>
	/// Verifies that ApplyPaging with zero take throws.
	/// </summary>
	[Fact]
	public void Specification_ApplyPaging_ZeroTake_ShouldThrow()
	{
		var spec = new TestSpecification();
		var act = () => spec.SetPaging(0, 0);
		act.Should().Throw<ArgumentOutOfRangeException>();
	}

	[Fact]
	public void Specification_ApplyPaging_NegativeTake_ShouldThrow()
	{
		var spec = new TestSpecification();
		var act = () => spec.SetPaging(0, -5);
		act.Should().Throw<ArgumentOutOfRangeException>();
	}

	#endregion

	#region ReadModelSpecification — Ordering

	[Fact]
	public void Specification_AddOrderBy_ShouldSetAscendingAndClearDescending()
	{
		var spec = new TestSpecification();
		spec.SetOrderByDescending(x => x.Name);
		spec.SetOrderBy(x => x.Id);

		spec.OrderByExpression.Should().NotBeNull();
		spec.OrderByDescendingExpression.Should().BeNull();
	}

	[Fact]
	public void Specification_AddOrderByDescending_ShouldSetDescendingAndClearAscending()
	{
		var spec = new TestSpecification();
		spec.SetOrderBy(x => x.Id);
		spec.SetOrderByDescending(x => x.Name);

		spec.OrderByDescendingExpression.Should().NotBeNull();
		spec.OrderByExpression.Should().BeNull();
	}

	[Fact]
	public void Specification_AddOrderBy_Null_ShouldThrow()
	{
		var spec = new TestSpecification();
		var act = () => spec.SetOrderBy(null!);
		act.Should().Throw<ArgumentNullException>();
	}

	[Fact]
	public void Specification_AddOrderByDescending_Null_ShouldThrow()
	{
		var spec = new TestSpecification();
		var act = () => spec.SetOrderByDescending(null!);
		act.Should().Throw<ArgumentNullException>();
	}

	#endregion

	#region ProjectionHandler — MediatR Integration

	[Fact]
	public async Task ProjectionHandler_HandleWithResultAsync_ShouldDelegateToProjectAsync()
	{
		var handler = new TestProjection();
		var domainEvent = new TestProjectionEvent("test-data");

		var result = await handler.HandleWithResultAsync(domainEvent, CancellationToken.None);

		result.IsSuccess.Should().BeTrue();
		handler.ReceivedEvent.Should().Be(domainEvent);
		handler.ReceivedEvent!.Data.Should().Be("test-data");
		handler.CallCount.Should().Be(1);
	}

	[Fact]
	public async Task ProjectionHandler_Handle_ViaNotification_ShouldDelegateToProjectAsync()
	{
		var handler = new TestProjection();
		var domainEvent = new TestProjectionEvent("test-data");
		var notification = new DomainEventNotification<TestProjectionEvent>(domainEvent);
		IDomainEventHandler<TestProjectionEvent> domainEventHandler = handler;

		await domainEventHandler.Handle(notification, CancellationToken.None);

		handler.ReceivedEvent.Should().Be(domainEvent);
		handler.CallCount.Should().Be(1);
	}

	[Fact]
	public void ProjectionHandler_ShouldImplementIProjectionHandler()
	{
		var handler = new TestProjection();

		handler.Should().BeAssignableTo<IProjectionHandler<TestProjectionEvent>>();
		handler.Should().BeAssignableTo<IProjectionHandler>();
	}

	[Fact]
	public async Task ProjectionHandler_AsIProjectionHandler_ShouldCallProjectAsync()
	{
		var handler = new TestProjection();
		var domainEvent = new TestProjectionEvent("interface-test");

		IProjectionHandler<TestProjectionEvent> projectionHandler = handler;
		await projectionHandler.ProjectAsync(domainEvent, CancellationToken.None);

		handler.ReceivedEvent.Should().Be(domainEvent);
		handler.CallCount.Should().Be(1);
	}

	[Fact]
	public async Task ProjectionHandler_Idempotency_MultipleCallsSameEvent_ShouldAllSucceed()
	{
		var handler = new TestProjection();
		var domainEvent = new TestProjectionEvent("idempotent-test");

		await handler.HandleWithResultAsync(domainEvent, CancellationToken.None);
		await handler.HandleWithResultAsync(domainEvent, CancellationToken.None);
		await handler.HandleWithResultAsync(domainEvent, CancellationToken.None);

		handler.CallCount.Should().Be(3);
	}

	#endregion
}
