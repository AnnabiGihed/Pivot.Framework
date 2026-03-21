using FluentAssertions;

namespace Pivot.Framework.Domain.Tests;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Unit tests for <see cref="AssemblyReference"/>.
///              Verifies the assembly reference is correctly exposed.
/// </summary>
public class AssemblyReferenceTests
{
	#region Assembly Tests
	/// <summary>
	/// Verifies that <see cref="AssemblyReference.Assembly"/> returns the Domain assembly.
	/// </summary>
	[Fact]
	public void Assembly_ShouldReturnDomainAssembly()
	{
		var assembly = AssemblyReference.Assembly;

		assembly.Should().NotBeNull();
		assembly.GetName().Name.Should().Be("Pivot.Framework.Domain");
	}
	#endregion
}
