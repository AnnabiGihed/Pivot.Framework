using System.Reflection;

namespace Templates.Core.Domain;

public static class AssemblyReference
{
	public static readonly Assembly Assembly = typeof(AssemblyReference).Assembly;
}
