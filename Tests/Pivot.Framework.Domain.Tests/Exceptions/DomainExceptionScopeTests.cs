using FluentAssertions;
using Pivot.Framework.Domain.Exceptions;

namespace Pivot.Framework.Domain.Tests.Exceptions;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Unit tests for <see cref="DomainExceptionScope"/>.
///              Verifies that the scope correctly collects exceptions and throws
///              <see cref="AggregateDomainException"/> on <see cref="DomainExceptionScope.ThrowIfAny"/>
///              or <see cref="IDisposable.Dispose"/>.
/// </summary>
public class DomainExceptionScopeTests
{
	#region ThrowIfAny Tests
	/// <summary>
	/// Verifies that <see cref="DomainExceptionScope.ThrowIfAny"/> does not throw
	/// when no exceptions have been collected.
	/// </summary>
	[Fact]
	public void ThrowIfAny_WithNoExceptions_ShouldNotThrow()
	{
		var scope = new DomainExceptionScope();

		var act = () => scope.ThrowIfAny();

		act.Should().NotThrow();
	}

	/// <summary>
	/// Verifies that <see cref="DomainExceptionScope.ThrowIfAny"/> throws
	/// <see cref="AggregateDomainException"/> when exceptions have been collected.
	/// </summary>
	[Fact]
	public void ThrowIfAny_WithExceptions_ShouldThrowAggregateDomainException()
	{
		var scope = new DomainExceptionScope();
		scope.AddException(new RequiredDomainException("field"));

		var act = () => scope.ThrowIfAny();

		act.Should().Throw<AggregateDomainException>();
	}
	#endregion

	#region Dispose Tests
	/// <summary>
	/// Verifies that disposing the scope throws <see cref="AggregateDomainException"/>
	/// when exceptions have been collected.
	/// </summary>
	[Fact]
	public void Dispose_WithExceptions_ShouldThrowAggregateDomainException()
	{
		var act = () =>
		{
			using var scope = new DomainExceptionScope();
			scope.AddException(new RequiredDomainException("field"));
		};

		act.Should().Throw<AggregateDomainException>();
	}

	/// <summary>
	/// Verifies that disposing the scope does not throw when no exceptions have been collected.
	/// </summary>
	[Fact]
	public void Dispose_WithNoExceptions_ShouldNotThrow()
	{
		var act = () =>
		{
			using var scope = new DomainExceptionScope();
		};

		act.Should().NotThrow();
	}
	#endregion
}
