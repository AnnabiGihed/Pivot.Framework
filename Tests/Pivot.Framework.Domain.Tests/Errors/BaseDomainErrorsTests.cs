using FluentAssertions;
using Pivot.Framework.Domain.Errors;
using Pivot.Framework.Domain.Shared;

namespace Pivot.Framework.Domain.Tests.Errors;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Unit tests for <see cref="BaseDomainErrors"/>.
///              Verifies all factory methods in the General and ValueObjects error catalogs.
/// </summary>
public class BaseDomainErrorsTests
{
	#region NotFound Tests
	/// <summary>
	/// Verifies NotFound without ID returns a valid error.
	/// </summary>
	[Fact]
	public void NotFound_WithoutId_ShouldReturnError()
	{
		var error = BaseDomainErrors.General.NotFound();

		error.Should().NotBeNull();
		error.Code.Should().NotBeNullOrWhiteSpace();
		error.Message.Should().NotBeNullOrWhiteSpace();
	}

	/// <summary>
	/// Verifies NotFound with ID includes the ID in the message.
	/// </summary>
	[Fact]
	public void NotFound_WithId_ShouldContainId()
	{
		var id = Guid.NewGuid();

		var error = BaseDomainErrors.General.NotFound(id);

		error.Message.Should().Contain(id.ToString());
	}

	/// <summary>
	/// Verifies NotFound with custom descriptors uses them.
	/// </summary>
	[Fact]
	public void NotFound_WithDescriptors_ShouldUseCustomCode()
	{
		var error = BaseDomainErrors.General.NotFound(null, "Custom.NotFound", "Record not found{0}");

		error.Code.Should().Be("Custom.NotFound");
	}
	#endregion

	#region ValueIsInvalid Tests
	/// <summary>
	/// Verifies ValueIsInvalid returns a valid error.
	/// </summary>
	[Fact]
	public void ValueIsInvalid_ShouldReturnError()
	{
		var error = BaseDomainErrors.General.ValueIsInvalid();

		error.Should().NotBeNull();
		error.Code.Should().NotBeNullOrWhiteSpace();
	}

	/// <summary>
	/// Verifies ValueIsInvalid with name and value includes them.
	/// </summary>
	[Fact]
	public void ValueIsInvalid_WithNameAndValue_ShouldContainBoth()
	{
		var error = BaseDomainErrors.General.ValueIsInvalid("Email", "invalid");

		error.Message.Should().Contain("Email");
		error.Message.Should().Contain("invalid");
	}

	/// <summary>
	/// Verifies ValueIsInvalid with custom descriptors.
	/// </summary>
	[Fact]
	public void ValueIsInvalid_WithDescriptors_ShouldUseCustomCode()
	{
		var error = BaseDomainErrors.General.ValueIsInvalid("f", "v", "Custom.Invalid", "Invalid{0}");

		error.Code.Should().Be("Custom.Invalid");
	}
	#endregion

	#region ValueAlreadyExists Tests
	/// <summary>
	/// Verifies ValueAlreadyExists returns a valid error.
	/// </summary>
	[Fact]
	public void ValueAlreadyExists_ShouldReturnError()
	{
		var error = BaseDomainErrors.General.ValueAlreadyExists();

		error.Should().NotBeNull();
		error.Code.Should().NotBeNullOrWhiteSpace();
	}

	/// <summary>
	/// Verifies ValueAlreadyExists with custom descriptors.
	/// </summary>
	[Fact]
	public void ValueAlreadyExists_WithDescriptors_ShouldUseCustomCode()
	{
		var error = BaseDomainErrors.General.ValueAlreadyExists("f", "v", "Custom.Exists", "Exists{0}");

		error.Code.Should().Be("Custom.Exists");
	}
	#endregion

	#region ValueIsToLong Tests
	/// <summary>
	/// Verifies ValueIsToLong returns a valid error.
	/// </summary>
	[Fact]
	public void ValueIsToLong_ShouldReturnError()
	{
		var error = BaseDomainErrors.General.ValueIsToLong("Name", "value", 50);

		error.Should().NotBeNull();
		error.Code.Should().NotBeNullOrWhiteSpace();
	}

	/// <summary>
	/// Verifies ValueIsToLong with custom descriptors.
	/// </summary>
	[Fact]
	public void ValueIsToLong_WithDescriptors_ShouldUseCustomCode()
	{
		var error = BaseDomainErrors.General.ValueIsToLong("f", "v", 10, "Custom.Long", "Too long{0}{1}");

		error.Code.Should().Be("Custom.Long");
	}
	#endregion

	#region ValueIsToShort Tests
	/// <summary>
	/// Verifies ValueIsToShort returns a valid error.
	/// </summary>
	[Fact]
	public void ValueIsToShort_ShouldReturnError()
	{
		var error = BaseDomainErrors.General.ValueIsToShort("Name", "v", 5);

		error.Should().NotBeNull();
		error.Code.Should().NotBeNullOrWhiteSpace();
	}

	/// <summary>
	/// Verifies ValueIsToShort with custom descriptors.
	/// </summary>
	[Fact]
	public void ValueIsToShort_WithDescriptors_ShouldUseCustomCode()
	{
		var error = BaseDomainErrors.General.ValueIsToShort("f", "v", 5, "Custom.Short", "Too short{0}{1}");

		error.Code.Should().Be("Custom.Short");
	}
	#endregion

	#region ValueIsRequired Tests
	/// <summary>
	/// Verifies ValueIsRequired returns a valid error.
	/// </summary>
	[Fact]
	public void ValueIsRequired_ShouldReturnError()
	{
		var error = BaseDomainErrors.General.ValueIsRequired();

		error.Should().NotBeNull();
		error.Code.Should().NotBeNullOrWhiteSpace();
	}

	/// <summary>
	/// Verifies ValueIsRequired with name includes it.
	/// </summary>
	[Fact]
	public void ValueIsRequired_WithName_ShouldContainName()
	{
		var error = BaseDomainErrors.General.ValueIsRequired("Email");

		error.Message.Should().Contain("Email");
	}

	/// <summary>
	/// Verifies ValueIsRequired with custom descriptors.
	/// </summary>
	[Fact]
	public void ValueIsRequired_WithDescriptors_ShouldUseCustomCode()
	{
		var error = BaseDomainErrors.General.ValueIsRequired("f", "Custom.Required", "Required{0}");

		error.Code.Should().Be("Custom.Required");
	}
	#endregion

	#region ValueNotNegative Tests
	/// <summary>
	/// Verifies ValueNotNegative returns a valid error.
	/// </summary>
	[Fact]
	public void ValueNotNegative_ShouldReturnError()
	{
		var error = BaseDomainErrors.General.ValueNotNegative();

		error.Should().NotBeNull();
		error.Code.Should().NotBeNullOrWhiteSpace();
	}

	/// <summary>
	/// Verifies ValueNotNegative with custom descriptors.
	/// </summary>
	[Fact]
	public void ValueNotNegative_WithDescriptors_ShouldUseCustomCode()
	{
		var error = BaseDomainErrors.General.ValueNotNegative("f", "v", "Custom.Neg", "Negative{0}");

		error.Code.Should().Be("Custom.Neg");
	}
	#endregion

	#region InvalidLength Tests
	/// <summary>
	/// Verifies InvalidLength returns a valid error.
	/// </summary>
	[Fact]
	public void InvalidLength_ShouldReturnError()
	{
		var error = BaseDomainErrors.General.InvalidLength();

		error.Should().NotBeNull();
		error.Code.Should().NotBeNullOrWhiteSpace();
	}

	/// <summary>
	/// Verifies InvalidLength with custom descriptors.
	/// </summary>
	[Fact]
	public void InvalidLength_WithDescriptors_ShouldUseCustomCode()
	{
		var error = BaseDomainErrors.General.InvalidLength("f", "Custom.Length", "Invalid{0}");

		error.Code.Should().Be("Custom.Length");
	}
	#endregion

	#region CollectionIsTooSmall Tests
	/// <summary>
	/// Verifies CollectionIsTooSmall returns a valid error.
	/// </summary>
	[Fact]
	public void CollectionIsTooSmall_ShouldReturnError()
	{
		var error = BaseDomainErrors.General.CollectionIsTooSmall(5, 2);

		error.Should().NotBeNull();
		error.Code.Should().NotBeNullOrWhiteSpace();
	}

	/// <summary>
	/// Verifies CollectionIsTooSmall with custom descriptors.
	/// </summary>
	[Fact]
	public void CollectionIsTooSmall_WithDescriptors_ShouldUseCustomCode()
	{
		var error = BaseDomainErrors.General.CollectionIsTooSmall(5, 2, "Custom.Small", "Too small {0} {1}");

		error.Code.Should().Be("Custom.Small");
	}
	#endregion

	#region CollectionIsTooLarge Tests
	/// <summary>
	/// Verifies CollectionIsTooLarge returns a valid error.
	/// </summary>
	[Fact]
	public void CollectionIsTooLarge_ShouldReturnError()
	{
		var error = BaseDomainErrors.General.CollectionIsTooLarge(10, 15);

		error.Should().NotBeNull();
		error.Code.Should().NotBeNullOrWhiteSpace();
	}

	/// <summary>
	/// Verifies CollectionIsTooLarge with custom descriptors.
	/// </summary>
	[Fact]
	public void CollectionIsTooLarge_WithDescriptors_ShouldUseCustomCode()
	{
		var error = BaseDomainErrors.General.CollectionIsTooLarge(10, 15, "Custom.Large", "Too large {0} {1}");

		error.Code.Should().Be("Custom.Large");
	}
	#endregion

	#region UnexpectedError Tests
	/// <summary>
	/// Verifies UnexpectedError returns a valid error.
	/// </summary>
	[Fact]
	public void UnexpectedError_ShouldReturnError()
	{
		var error = BaseDomainErrors.General.UnexpectedError("Something broke");

		error.Should().NotBeNull();
		error.Code.Should().NotBeNullOrWhiteSpace();
		error.Message.Should().Be("Something broke");
	}

	/// <summary>
	/// Verifies UnexpectedError with null message throws.
	/// </summary>
	[Fact]
	public void UnexpectedError_WithNullMessage_ShouldThrow()
	{
		var act = () => BaseDomainErrors.General.UnexpectedError(null!);

		act.Should().Throw<ArgumentNullException>();
	}

	/// <summary>
	/// Verifies UnexpectedError with custom descriptors.
	/// </summary>
	[Fact]
	public void UnexpectedError_WithDescriptors_ShouldUseCustomCode()
	{
		var error = BaseDomainErrors.General.UnexpectedError("msg", "Custom.ISE", "Error: {0}");

		error.Code.Should().Be("Custom.ISE");
	}

	/// <summary>
	/// Verifies UnexpectedError with descriptors and null message throws.
	/// </summary>
	[Fact]
	public void UnexpectedError_WithDescriptors_NullMessage_ShouldThrow()
	{
		var act = () => BaseDomainErrors.General.UnexpectedError(null!, "code", "msg");

		act.Should().Throw<ArgumentNullException>();
	}
	#endregion

	#region CurrencyMismatch Tests
	/// <summary>
	/// Verifies CurrencyMismatch returns a valid error.
	/// </summary>
	[Fact]
	public void CurrencyMismatch_ShouldReturnError()
	{
		var error = BaseDomainErrors.ValueObjects.Money.CurrencyMismatch("Amount", "EUR", "USD");

		error.Should().NotBeNull();
		error.Code.Should().NotBeNullOrWhiteSpace();
	}

	/// <summary>
	/// Verifies CurrencyMismatch with custom descriptors.
	/// </summary>
	[Fact]
	public void CurrencyMismatch_WithDescriptors_ShouldUseCustomCode()
	{
		var error = BaseDomainErrors.ValueObjects.Money.CurrencyMismatch(
			"Amount", "EUR", "USD", "Custom.Currency", "Mismatch{0}");

		error.Code.Should().Be("Custom.Currency");
	}
	#endregion
}
