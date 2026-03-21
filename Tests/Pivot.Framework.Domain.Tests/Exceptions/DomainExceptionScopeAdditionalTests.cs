using FluentAssertions;
using Pivot.Framework.Domain.Exceptions;

namespace Pivot.Framework.Domain.Tests.Exceptions;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Additional unit tests for <see cref="DomainExceptionScope"/>.
///              Covers AddExceptions, null guards, multiple exceptions, and scope reuse.
/// </summary>
public class DomainExceptionScopeAdditionalTests
{
	#region AddException Tests
	/// <summary>
	/// Verifies that <see cref="DomainExceptionScope.AddException"/> with null throws.
	/// </summary>
	[Fact]
	public void AddException_WithNull_ShouldThrow()
	{
		var scope = new DomainExceptionScope();

		var act = () => scope.AddException(null!);

		act.Should().Throw<ArgumentNullException>();
	}
	#endregion

	#region AddExceptions Tests
	/// <summary>
	/// Verifies that <see cref="DomainExceptionScope.AddExceptions"/> adds multiple exceptions.
	/// </summary>
	[Fact]
	public void AddExceptions_ShouldCollectMultiple()
	{
		var scope = new DomainExceptionScope();

		scope.AddExceptions(new DomainException[]
		{
			new RequiredDomainException("field1"),
			new NotExistsDomainException("field2")
		});

		var act = () => scope.ThrowIfAny();

		act.Should().Throw<AggregateDomainException>()
			.Which.DomainExceptions.Should().HaveCount(2);
	}

	/// <summary>
	/// Verifies that <see cref="DomainExceptionScope.AddExceptions"/> with null throws.
	/// </summary>
	[Fact]
	public void AddExceptions_WithNull_ShouldThrow()
	{
		var scope = new DomainExceptionScope();

		var act = () => scope.AddExceptions(null!);

		act.Should().Throw<ArgumentNullException>();
	}

	/// <summary>
	/// Verifies that null entries in the collection are filtered out.
	/// </summary>
	[Fact]
	public void AddExceptions_WithNullEntries_ShouldFilterThem()
	{
		var scope = new DomainExceptionScope();

		scope.AddExceptions(new DomainException[]
		{
			new RequiredDomainException("field1"),
			null!,
			new NotExistsDomainException("field2")
		});

		var act = () => scope.ThrowIfAny();

		act.Should().Throw<AggregateDomainException>()
			.Which.DomainExceptions.Should().HaveCount(2);
	}
	#endregion

	#region ThrowIfAny Clears Collection Tests
	/// <summary>
	/// Verifies that ThrowIfAny clears the collection after throwing.
	/// </summary>
	[Fact]
	public void ThrowIfAny_ShouldClearCollectionAfterThrowing()
	{
		var scope = new DomainExceptionScope();
		scope.AddException(new RequiredDomainException("field"));

		try { scope.ThrowIfAny(); } catch { }

		// Second call should not throw since collection was cleared
		var act = () => scope.ThrowIfAny();
		act.Should().NotThrow();
	}
	#endregion

	#region Multiple Exception Types Tests
	/// <summary>
	/// Verifies that scope collects various exception subtypes correctly.
	/// </summary>
	[Fact]
	public void Scope_ShouldCollectVariousExceptionTypes()
	{
		var scope = new DomainExceptionScope();
		scope.AddException(new RequiredDomainException("f1"));
		scope.AddException(new NotExistsDomainException("f2"));
		scope.AddException(new AlreadyExistsDomainException("f3"));
		scope.AddException(new OutOfRangeDomainException("f4"));
		scope.AddException(new AtLeastOneIsRequiredDomainException("f5"));

		var act = () => scope.ThrowIfAny();

		act.Should().Throw<AggregateDomainException>()
			.Which.DomainExceptions.Should().HaveCount(5);
	}
	#endregion
}
