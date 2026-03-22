using System.Reflection;

namespace Pivot.Framework.Application;

/// <summary>
/// Provides a static reference to the Application assembly for use in reflection-based
/// scenarios such as MediatR handler scanning and Scrutor service registration.
/// </summary>
public static class AssemblyReference
{
	/// <summary>
	/// The <see cref="System.Reflection.Assembly"/> instance for the Application project.
	/// </summary>
	public static readonly Assembly Assembly = typeof(AssemblyReference).Assembly;
}

