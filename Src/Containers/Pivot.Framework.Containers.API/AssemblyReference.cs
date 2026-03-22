using System.Reflection;

namespace Pivot.Framework.Containers.API;

/// <summary>
/// Provides a static reference to the Containers.API assembly for use in reflection-based
/// scenarios such as MediatR handler scanning and Scrutor service registration.
/// </summary>
public static class AssemblyReference
{
	/// <summary>
	/// The <see cref="System.Reflection.Assembly"/> instance for the Containers.API project.
	/// </summary>
	public static readonly Assembly Assembly = typeof(AssemblyReference).Assembly;
}
