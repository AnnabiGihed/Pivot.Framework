using FluentAssertions;
using Pivot.Framework.Application.Exceptions;

namespace Pivot.Framework.Application.Tests.Exceptions;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Unit tests for <see cref="NotFoundException"/>.
///              Verifies construction, message formatting, guards, and ToError factory.
/// </summary>
public class NotFoundExceptionTests
{
	#region Constructor Tests
	/// <summary>
	/// Verifies that the constructor assigns properties and formats message.
	/// </summary>
	[Fact]
	public void Constructor_ShouldAssignPropertiesAndFormatMessage()
	{
		var key = Guid.NewGuid();

		var ex = new NotFoundException("Order", key);

		ex.ResourceName.Should().Be("Order");
		ex.ResourceKey.Should().Be(key);
		ex.Message.Should().Be($"Order ({key}) was not found.");
	}

	/// <summary>
	/// Verifies that null name throws.
	/// </summary>
	[Fact]
	public void Constructor_NullName_ShouldThrow()
	{
		var act = () => new NotFoundException(null!, "key");

		act.Should().Throw<ArgumentNullException>();
	}

	/// <summary>
	/// Verifies that empty name throws.
	/// </summary>
	[Fact]
	public void Constructor_EmptyName_ShouldThrow()
	{
		var act = () => new NotFoundException("", "key");

		act.Should().Throw<ArgumentNullException>();
	}

	/// <summary>
	/// Verifies that null key throws.
	/// </summary>
	[Fact]
	public void Constructor_NullKey_ShouldThrow()
	{
		var act = () => new NotFoundException("Order", null!);

		act.Should().Throw<ArgumentNullException>();
	}
	#endregion

	#region ToError Tests
	/// <summary>
	/// Verifies that <see cref="NotFoundException.ToError"/> creates an Error with correct code and message.
	/// </summary>
	[Fact]
	public void ToError_ShouldCreateErrorWithNotFoundCode()
	{
		var key = Guid.NewGuid();

		var error = NotFoundException.ToError("Order", key);

		error.Code.Should().Be("Error.NotFound");
		error.Message.Should().Contain("Order");
		error.Message.Should().Contain(key.ToString());
	}
	#endregion

	#region Inheritance Tests
	/// <summary>
	/// Verifies that NotFoundException inherits from Exception.
	/// </summary>
	[Fact]
	public void NotFoundException_ShouldInheritFromException()
	{
		var ex = new NotFoundException("Order", 1);

		ex.Should().BeAssignableTo<Exception>();
	}
	#endregion
}
