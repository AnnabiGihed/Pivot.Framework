using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using NSubstitute;
using Pivot.Framework.Application.Behaviors;
using Pivot.Framework.Domain.Shared;
using FvValidationResult = FluentValidation.Results.ValidationResult;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace Pivot.Framework.Application.Tests.Behaviors;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Unit tests for <see cref="ValidationPipelineBehavior{TRequest, TResponse}"/>.
///              Verifies no-validator pass-through, validation error aggregation,
///              distinct error deduplication, and generic Result{T} support.
/// </summary>
public class ValidationPipelineBehaviorTests
{
	#region Test Infrastructure
	internal record TestCommand(string Name) : IRequest<Result>;
	internal record TestQueryCommand(int Id) : IRequest<Result<string>>;
	#endregion

	#region Constructor Tests
	/// <summary>
	/// Verifies that null validators throws.
	/// </summary>
	[Fact]
	public void Constructor_NullValidators_ShouldThrow()
	{
		var act = () => new ValidationPipelineBehavior<TestCommand, Result>(null!);

		act.Should().Throw<ArgumentNullException>();
	}
	#endregion

	#region No Validators Tests
	/// <summary>
	/// Verifies that when no validators are registered, next is called directly.
	/// </summary>
	[Fact]
	public async Task Handle_NoValidators_ShouldCallNext()
	{
		var behavior = new ValidationPipelineBehavior<TestCommand, Result>(
			Array.Empty<IValidator<TestCommand>>());

		var result = await behavior.Handle(
			new TestCommand("Test"),
			ct => Task.FromResult(Result.Success()),
			CancellationToken.None);

		result.IsSuccess.Should().BeTrue();
	}
	#endregion

	#region Validation Error Tests
	/// <summary>
	/// Verifies that validation errors produce a validation failure result.
	/// </summary>
	[Fact]
	public async Task Handle_WithErrors_ShouldReturnValidationResult()
	{
		var validator = Substitute.For<IValidator<TestCommand>>();
		validator.ValidateAsync(Arg.Any<ValidationContext<TestCommand>>(), Arg.Any<CancellationToken>())
			.Returns(new FvValidationResult(new[]
			{
				new ValidationFailure("Name", "Name is required")
			}));

		var behavior = new ValidationPipelineBehavior<TestCommand, Result>(
			new[] { validator });

		var result = await behavior.Handle(
			new TestCommand(""),
			ct => Task.FromResult(Result.Success()),
			CancellationToken.None);

		result.IsFailure.Should().BeTrue();
		result.Should().BeAssignableTo<IValidationResult>();
	}

	/// <summary>
	/// Verifies that duplicate errors are deduplicated.
	/// </summary>
	[Fact]
	public async Task Handle_DuplicateErrors_ShouldDeduplicate()
	{
		var validator1 = Substitute.For<IValidator<TestCommand>>();
		validator1.ValidateAsync(Arg.Any<ValidationContext<TestCommand>>(), Arg.Any<CancellationToken>())
			.Returns(new FvValidationResult(new[]
			{
				new ValidationFailure("Name", "Name is required")
			}));

		var validator2 = Substitute.For<IValidator<TestCommand>>();
		validator2.ValidateAsync(Arg.Any<ValidationContext<TestCommand>>(), Arg.Any<CancellationToken>())
			.Returns(new FvValidationResult(new[]
			{
				new ValidationFailure("Name", "Name is required")
			}));

		var behavior = new ValidationPipelineBehavior<TestCommand, Result>(
			new[] { validator1, validator2 });

		var result = await behavior.Handle(
			new TestCommand(""),
			ct => Task.FromResult(Result.Success()),
			CancellationToken.None);

		result.IsFailure.Should().BeTrue();
		var validationResult = result as IValidationResult;
		validationResult.Should().NotBeNull();
		validationResult!.Errors.Should().HaveCount(1);
	}

	/// <summary>
	/// Verifies that when validators pass, next is called.
	/// </summary>
	[Fact]
	public async Task Handle_NoErrors_ShouldCallNext()
	{
		var validator = Substitute.For<IValidator<TestCommand>>();
		validator.ValidateAsync(Arg.Any<ValidationContext<TestCommand>>(), Arg.Any<CancellationToken>())
			.Returns(new FvValidationResult());

		var behavior = new ValidationPipelineBehavior<TestCommand, Result>(
			new[] { validator });

		var result = await behavior.Handle(
			new TestCommand("Valid"),
			ct => Task.FromResult(Result.Success()),
			CancellationToken.None);

		result.IsSuccess.Should().BeTrue();
	}
	#endregion

	#region Generic Result<T> Tests
	/// <summary>
	/// Verifies that validation errors work with Result{T} response type.
	/// </summary>
	[Fact]
	public async Task Handle_GenericResult_WithErrors_ShouldReturnValidationResult()
	{
		var validator = Substitute.For<IValidator<TestQueryCommand>>();
		validator.ValidateAsync(Arg.Any<ValidationContext<TestQueryCommand>>(), Arg.Any<CancellationToken>())
			.Returns(new FvValidationResult(new[]
			{
				new ValidationFailure("Id", "Id must be positive")
			}));

		var behavior = new ValidationPipelineBehavior<TestQueryCommand, Result<string>>(
			new[] { validator });

		var result = await behavior.Handle(
			new TestQueryCommand(-1),
			ct => Task.FromResult(Result.Success("OK")),
			CancellationToken.None);

		result.IsFailure.Should().BeTrue();
		result.Should().BeAssignableTo<IValidationResult>();
	}

	/// <summary>
	/// Verifies that valid generic Result{T} calls next.
	/// </summary>
	[Fact]
	public async Task Handle_GenericResult_NoErrors_ShouldCallNext()
	{
		var validator = Substitute.For<IValidator<TestQueryCommand>>();
		validator.ValidateAsync(Arg.Any<ValidationContext<TestQueryCommand>>(), Arg.Any<CancellationToken>())
			.Returns(new FvValidationResult());

		var behavior = new ValidationPipelineBehavior<TestQueryCommand, Result<string>>(
			new[] { validator });

		var result = await behavior.Handle(
			new TestQueryCommand(1),
			ct => Task.FromResult(Result.Success("OK")),
			CancellationToken.None);

		result.IsSuccess.Should().BeTrue();
		result.Value.Should().Be("OK");
	}
	#endregion

	#region Null Guard Tests
	/// <summary>
	/// Verifies that null request throws.
	/// </summary>
	[Fact]
	public async Task Handle_NullRequest_ShouldThrow()
	{
		var behavior = new ValidationPipelineBehavior<TestCommand, Result>(
			Array.Empty<IValidator<TestCommand>>());

		var act = () => behavior.Handle(
			null!,
			ct => Task.FromResult(Result.Success()),
			CancellationToken.None);

		await act.Should().ThrowAsync<ArgumentNullException>();
	}

	/// <summary>
	/// Verifies that null next throws.
	/// </summary>
	[Fact]
	public async Task Handle_NullNext_ShouldThrow()
	{
		var behavior = new ValidationPipelineBehavior<TestCommand, Result>(
			Array.Empty<IValidator<TestCommand>>());

		var act = () => behavior.Handle(
			new TestCommand("Test"),
			null!,
			CancellationToken.None);

		await act.Should().ThrowAsync<ArgumentNullException>();
	}
	#endregion

	#region Multiple Errors Tests
	/// <summary>
	/// Verifies that errors from multiple validators are aggregated.
	/// </summary>
	[Fact]
	public async Task Handle_MultipleValidators_ShouldAggregateErrors()
	{
		var validator1 = Substitute.For<IValidator<TestCommand>>();
		validator1.ValidateAsync(Arg.Any<ValidationContext<TestCommand>>(), Arg.Any<CancellationToken>())
			.Returns(new FvValidationResult(new[]
			{
				new ValidationFailure("Name", "Name is required")
			}));

		var validator2 = Substitute.For<IValidator<TestCommand>>();
		validator2.ValidateAsync(Arg.Any<ValidationContext<TestCommand>>(), Arg.Any<CancellationToken>())
			.Returns(new FvValidationResult(new[]
			{
				new ValidationFailure("Email", "Email is invalid")
			}));

		var behavior = new ValidationPipelineBehavior<TestCommand, Result>(
			new[] { validator1, validator2 });

		var result = await behavior.Handle(
			new TestCommand(""),
			ct => Task.FromResult(Result.Success()),
			CancellationToken.None);

		result.IsFailure.Should().BeTrue();
		var validationResult = result as IValidationResult;
		validationResult!.Errors.Should().HaveCount(2);
	}
	#endregion
}
