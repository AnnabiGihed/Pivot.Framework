using FluentAssertions;
using Pivot.Framework.Domain.Exceptions;

namespace Pivot.Framework.Domain.Tests.Exceptions;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Unit tests for the <see cref="DomainException"/> hierarchy.
///              Verifies correct base class inheritance, <see cref="DomainException.ParameterName"/>
///              exposure, constructor guards, and all concrete exception subtypes.
/// </summary>
public class DomainExceptionTests
{
	#region Base Class Tests
	/// <summary>
	/// Verifies that <see cref="DomainException"/> inherits from <see cref="Exception"/>
	/// and NOT from <see cref="ArgumentException"/>.
	/// </summary>
	[Fact]
	public void DomainException_ShouldInheritFromException_NotArgumentException()
	{
		// Arrange & Act
		var ex = new RequiredDomainException("testParam", "Test message");

		// Assert
		ex.Should().BeAssignableTo<Exception>();
		ex.Should().NotBeAssignableTo<ArgumentException>();
	}

	/// <summary>
	/// Verifies that <see cref="DomainException.ParameterName"/> and <see cref="Exception.Message"/>
	/// are correctly assigned from constructor arguments.
	/// </summary>
	[Fact]
	public void DomainException_ShouldExpose_ParameterName()
	{
		// Arrange & Act
		var ex = new RequiredDomainException("myParam", "Something is required");

		// Assert
		ex.ParameterName.Should().Be("myParam");
		ex.Message.Should().Be("Something is required");
	}

	/// <summary>
	/// Verifies that all properties are preserved when constructing with an inner exception.
	/// </summary>
	[Fact]
	public void DomainException_WithInnerException_ShouldPreserveAll()
	{
		// Arrange
		var inner = new InvalidOperationException("inner failure");

		// Act
		var ex = new UnknownDomainException("field", "outer message");

		// Assert
		ex.ParameterName.Should().Be("field");
		ex.Message.Should().Be("outer message");
	}
	#endregion

	#region Constructor Guard Tests
	/// <summary>
	/// Verifies that null, empty, or whitespace parameter names are rejected.
	/// </summary>
	[Theory]
	[InlineData(null)]
	[InlineData("")]
	[InlineData("   ")]
	public void DomainException_ShouldThrow_WhenParameterNameIsNullOrWhitespace(string? parameterName)
	{
		// Act
		var act = () => new RequiredDomainException(parameterName!, "message");

		// Assert
		act.Should().Throw<ArgumentException>();
	}
	#endregion

	#region Inner Exception Constructor Tests
	/// <summary>
	/// Verifies that the inner exception constructor preserves all properties.
	/// </summary>
	[Fact]
	public void DomainException_InnerExceptionConstructor_ShouldPreserveAll()
	{
		var inner = new InvalidOperationException("inner failure");

		var ex = new TestDomainException("field", "outer message", inner);

		ex.ParameterName.Should().Be("field");
		ex.Message.Should().Be("outer message");
		ex.InnerException.Should().Be(inner);
	}

	/// <summary>
	/// Verifies that inner exception constructor rejects null parameterName.
	/// </summary>
	[Fact]
	public void DomainException_InnerExceptionConstructor_NullParamName_ShouldThrow()
	{
		var act = () => new TestDomainException(null!, "msg", new Exception());

		act.Should().Throw<ArgumentException>();
	}

	/// <summary>
	/// Verifies that inner exception constructor rejects null innerException.
	/// </summary>
	[Fact]
	public void DomainException_InnerExceptionConstructor_NullInnerException_ShouldThrow()
	{
		var act = () => new TestDomainException("param", "msg", null!);

		act.Should().Throw<ArgumentNullException>();
	}
	#endregion

	#region Subtype Tests
	/// <summary>
	/// Verifies that <see cref="RequiredDomainException"/> uses the default resource message
	/// when no explicit message is provided.
	/// </summary>
	[Fact]
	public void RequiredDomainException_DefaultMessage_ShouldUseResource()
	{
		// Arrange & Act
		var ex = new RequiredDomainException("field");

		// Assert
		ex.ParameterName.Should().Be("field");
		ex.Message.Should().NotBeNullOrWhiteSpace();
	}

	/// <summary>
	/// Verifies <see cref="AlreadyExistsDomainException"/> inherits from <see cref="DomainException"/>.
	/// </summary>
	[Fact]
	public void AlreadyExistsDomainException_ShouldInheritFromDomainException()
	{
		var ex = new AlreadyExistsDomainException("entity");
		ex.Should().BeAssignableTo<DomainException>();
		ex.ParameterName.Should().Be("entity");
	}

	/// <summary>
	/// Verifies <see cref="NotExistsDomainException"/> inherits from <see cref="DomainException"/>.
	/// </summary>
	[Fact]
	public void NotExistsDomainException_ShouldInheritFromDomainException()
	{
		var ex = new NotExistsDomainException("record");
		ex.Should().BeAssignableTo<DomainException>();
		ex.ParameterName.Should().Be("record");
	}

	/// <summary>
	/// Verifies <see cref="OutOfRangeDomainException"/> inherits from <see cref="DomainException"/>.
	/// </summary>
	[Fact]
	public void OutOfRangeDomainException_ShouldInheritFromDomainException()
	{
		var ex = new OutOfRangeDomainException("value");
		ex.Should().BeAssignableTo<DomainException>();
		ex.ParameterName.Should().Be("value");
	}

	/// <summary>
	/// Verifies <see cref="AtLeastOneIsRequiredDomainException"/> inherits from <see cref="DomainException"/>.
	/// </summary>
	[Fact]
	public void AtLeastOneIsRequiredDomainException_ShouldInheritFromDomainException()
	{
		var ex = new AtLeastOneIsRequiredDomainException("items");
		ex.Should().BeAssignableTo<DomainException>();
		ex.ParameterName.Should().Be("items");
	}

	/// <summary>
	/// Verifies <see cref="UnknownDomainException"/> default message constructor.
	/// </summary>
	[Fact]
	public void UnknownDomainException_DefaultMessage_ShouldUseResource()
	{
		var ex = new UnknownDomainException("field");

		ex.ParameterName.Should().Be("field");
		ex.Message.Should().NotBeNullOrWhiteSpace();
	}

	/// <summary>
	/// Verifies <see cref="UnknownDomainException"/> custom message constructor.
	/// </summary>
	[Fact]
	public void UnknownDomainException_CustomMessage_ShouldPreserveMessage()
	{
		var ex = new UnknownDomainException("field", "Custom error");

		ex.ParameterName.Should().Be("field");
		ex.Message.Should().Be("Custom error");
	}

	/// <summary>
	/// Verifies all subtypes with custom message constructor.
	/// </summary>
	[Fact]
	public void RequiredDomainException_CustomMessage_ShouldPreserve()
	{
		var ex = new RequiredDomainException("field", "Custom required");
		ex.Message.Should().Be("Custom required");
	}

	/// <summary>
	/// Verifies AlreadyExistsDomainException with custom message.
	/// </summary>
	[Fact]
	public void AlreadyExistsDomainException_CustomMessage_ShouldPreserve()
	{
		var ex = new AlreadyExistsDomainException("entity", "Already exists custom");
		ex.Message.Should().Be("Already exists custom");
	}

	/// <summary>
	/// Verifies NotExistsDomainException with custom message.
	/// </summary>
	[Fact]
	public void NotExistsDomainException_CustomMessage_ShouldPreserve()
	{
		var ex = new NotExistsDomainException("record", "Does not exist custom");
		ex.Message.Should().Be("Does not exist custom");
	}

	/// <summary>
	/// Verifies OutOfRangeDomainException with custom message.
	/// </summary>
	[Fact]
	public void OutOfRangeDomainException_CustomMessage_ShouldPreserve()
	{
		var ex = new OutOfRangeDomainException("value", "Out of range custom");
		ex.Message.Should().Be("Out of range custom");
	}

	/// <summary>
	/// Verifies AtLeastOneIsRequiredDomainException with custom message.
	/// </summary>
	[Fact]
	public void AtLeastOneIsRequiredDomainException_CustomMessage_ShouldPreserve()
	{
		var ex = new AtLeastOneIsRequiredDomainException("items", "At least one custom");
		ex.Message.Should().Be("At least one custom");
	}
	#endregion

	#region Test Doubles
	/// <summary>
	/// Test double exposing the protected inner exception constructor of DomainException.
	/// </summary>
	private sealed class TestDomainException : DomainException
	{
		public TestDomainException(string parameterName, string message)
			: base(parameterName, message) { }

		public TestDomainException(string parameterName, string message, Exception innerException)
			: base(parameterName, message, innerException) { }
	}
	#endregion
}
