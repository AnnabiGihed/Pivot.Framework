using System.Reflection;

namespace Pivot.Framework.Infrastructure.Persistence.EntityFrameworkCore;

/// <summary>
/// Provides a static reference to the Persistence.EntityFrameworkCore assembly for use in
/// reflection-based scenarios such as MediatR handler scanning and Scrutor service registration.
/// </summary>
public static class AssemblyReference
{
	/// <summary>
	/// The <see cref="System.Reflection.Assembly"/> instance for the Persistence.EntityFrameworkCore project.
	/// </summary>
	public static readonly Assembly Assembly = typeof(AssemblyReference).Assembly;
}
