using FluentAssertions;
using Pivot.Framework.Domain.Exceptions;

namespace Pivot.Framework.Domain.Tests.Exceptions;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Unit tests for <see cref="AggregateDomainException"/>.
///              Verifies correct inheritance, message formatting for single and multiple errors,
///              and the strongly-typed <see cref="AggregateDomainException.DomainExceptions"/> collection.
/// </summary>
public class AggregateDomainExceptionTests
{
	#region Inheritance Tests
	/// <summary>
	/// Verifies that <see cref="AggregateDomainException"/> inherits from <see cref="AggregateException"/>
	/// and exposes the correct number of domain exceptions.
	/// </summary>
	[Fact]
	public void AggregateDomainException_ShouldInheritFromAggregateException()
	{
		var exceptions = new List<DomainException>
		{
			new RequiredDomainException("field1"),
			new NotExistsDomainException("field2")
		};

		var ex = new AggregateDomainException(exceptions);

		ex.Should().BeAssignableTo<AggregateException>();
		ex.DomainExceptions.Should().HaveCount(2);
	}
	#endregion

	#region Message Formatting Tests
	/// <summary>
	/// Verifies that a single error uses its own message in the aggregate message.
	/// </summary>
	[Fact]
	public void AggregateDomainException_SingleError_ShouldUseItsMessage()
	{
		var exceptions = new List<DomainException>
		{
			new RequiredDomainException("name", "Name is required")
		};

		var ex = new AggregateDomainException(exceptions);

		ex.Message.Should().Contain("Name is required");
	}

	/// <summary>
	/// Verifies that multiple errors produce a count-based aggregate message.
	/// </summary>
	[Fact]
	public void AggregateDomainException_MultipleErrors_ShouldShowCount()
	{
		var exceptions = new List<DomainException>
		{
			new RequiredDomainException("a", "Error A"),
			new RequiredDomainException("b", "Error B"),
			new RequiredDomainException("c", "Error C")
		};

		var ex = new AggregateDomainException(exceptions);

		ex.Message.Should().Contain("3");
	}
	#endregion
}
