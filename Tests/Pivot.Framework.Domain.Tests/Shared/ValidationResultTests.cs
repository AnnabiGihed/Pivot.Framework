using FluentAssertions;
using Pivot.Framework.Domain.Shared;

namespace Pivot.Framework.Domain.Tests.Shared;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Unit tests for <see cref="ValidationResult"/> and <see cref="ValidationResult{TValue}"/>.
///              Verifies error collection, factory methods, and guard clauses.
/// </summary>
public class ValidationResultTests
{
	#region Non-Generic ValidationResult Tests
	/// <summary>
	/// Verifies that <see cref="ValidationResult.WithErrors(IEnumerable{Error})"/> creates a failure
	/// with the correct errors.
	/// </summary>
	[Fact]
	public void WithErrors_Enumerable_ShouldCreateFailureWithErrors()
	{
		var errors = new List<Error>
		{
			new("ERR1", "Error 1"),
			new("ERR2", "Error 2")
		};

		var result = ValidationResult.WithErrors(errors);

		result.IsFailure.Should().BeTrue();
		result.Errors.Should().HaveCount(2);
		result.Error.Should().Be(ValidationErrors.ValidationError);
	}

	/// <summary>
	/// Verifies that <see cref="ValidationResult.WithErrors(Error[])"/> params overload works.
	/// </summary>
	[Fact]
	public void WithErrors_Params_ShouldCreateFailureWithErrors()
	{
		var result = ValidationResult.WithErrors(
			new Error("ERR1", "Error 1"),
			new Error("ERR2", "Error 2"));

		result.IsFailure.Should().BeTrue();
		result.Errors.Should().HaveCount(2);
	}

	/// <summary>
	/// Verifies that null errors in the collection are filtered out.
	/// </summary>
	[Fact]
	public void WithErrors_ShouldFilterNullAndNoneErrors()
	{
		var errors = new List<Error>
		{
			new("ERR1", "Error 1"),
			null!,
			Error.None,
			new("ERR2", "Error 2")
		};

		var result = ValidationResult.WithErrors(errors);

		result.Errors.Should().HaveCount(2);
	}

	/// <summary>
	/// Verifies that passing only Error.None throws since no valid errors remain.
	/// </summary>
	[Fact]
	public void WithErrors_OnlyNoneErrors_ShouldThrow()
	{
		var errors = new List<Error> { Error.None };

		var act = () => ValidationResult.WithErrors(errors);

		act.Should().Throw<ArgumentException>();
	}

	/// <summary>
	/// Verifies that passing null collection throws.
	/// </summary>
	[Fact]
	public void WithErrors_NullCollection_ShouldThrow()
	{
		var act = () => ValidationResult.WithErrors((IEnumerable<Error>)null!);

		act.Should().Throw<ArgumentNullException>();
	}

	/// <summary>
	/// Verifies that passing empty collection throws.
	/// </summary>
	[Fact]
	public void WithErrors_EmptyCollection_ShouldThrow()
	{
		var act = () => ValidationResult.WithErrors(Array.Empty<Error>());

		act.Should().Throw<ArgumentException>();
	}

	/// <summary>
	/// Verifies that ValidationResult has BadRequest exception type.
	/// </summary>
	[Fact]
	public void WithErrors_ShouldHaveBadRequestExceptionType()
	{
		var result = ValidationResult.WithErrors(new Error("ERR", "msg"));

		result.ResultExceptionType.Should().Be(ResultExceptionType.BadRequest);
	}
	#endregion

	#region Generic ValidationResult<T> Tests
	/// <summary>
	/// Verifies that generic <see cref="ValidationResult{TValue}.WithErrors"/> creates a typed failure.
	/// </summary>
	[Fact]
	public void WithErrorsT_ShouldCreateTypedFailure()
	{
		var result = ValidationResult<int>.WithErrors(new Error("ERR", "msg"));

		result.IsFailure.Should().BeTrue();
		result.Errors.Should().HaveCount(1);
	}

	/// <summary>
	/// Verifies that accessing Value on a generic ValidationResult throws.
	/// </summary>
	[Fact]
	public void WithErrorsT_ValueAccess_ShouldThrow()
	{
		var result = ValidationResult<string>.WithErrors(new Error("ERR", "msg"));

		var act = () => result.Value;

		act.Should().Throw<InvalidOperationException>();
	}

	/// <summary>
	/// Verifies that generic ValidationResult params overload works.
	/// </summary>
	[Fact]
	public void WithErrorsT_Params_ShouldWork()
	{
		var result = ValidationResult<int>.WithErrors(
			new Error("ERR1", "msg1"),
			new Error("ERR2", "msg2"));

		result.Errors.Should().HaveCount(2);
	}

	/// <summary>
	/// Verifies that generic WithErrors filters null and None errors.
	/// </summary>
	[Fact]
	public void WithErrorsT_ShouldFilterNullAndNoneErrors()
	{
		var errors = new List<Error>
		{
			new("ERR1", "msg1"),
			null!,
			Error.None
		};

		var result = ValidationResult<int>.WithErrors(errors);

		result.Errors.Should().HaveCount(1);
	}

	/// <summary>
	/// Verifies that generic WithErrors throws for null collection.
	/// </summary>
	[Fact]
	public void WithErrorsT_NullCollection_ShouldThrow()
	{
		var act = () => ValidationResult<int>.WithErrors((IEnumerable<Error>)null!);

		act.Should().Throw<ArgumentNullException>();
	}

	/// <summary>
	/// Verifies that generic WithErrors throws for empty after filtering.
	/// </summary>
	[Fact]
	public void WithErrorsT_EmptyAfterFilter_ShouldThrow()
	{
		var act = () => ValidationResult<int>.WithErrors(Error.None);

		act.Should().Throw<ArgumentException>();
	}

	/// <summary>
	/// Verifies that ValidationResult implements IValidationResult.
	/// </summary>
	[Fact]
	public void ValidationResult_ShouldImplementIValidationResult()
	{
		var result = ValidationResult.WithErrors(new Error("ERR", "msg"));

		result.Should().BeAssignableTo<IValidationResult>();
	}

	/// <summary>
	/// Verifies that generic ValidationResult implements IValidationResult.
	/// </summary>
	[Fact]
	public void ValidationResultT_ShouldImplementIValidationResult()
	{
		var result = ValidationResult<int>.WithErrors(new Error("ERR", "msg"));

		result.Should().BeAssignableTo<IValidationResult>();
	}
	#endregion
}
