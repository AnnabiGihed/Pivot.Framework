using System.Reflection;

namespace Templates.Core.Application;

public static class AssemblyReference
{
	public static readonly Assembly Assembly = typeof(AssemblyReference).Assembly;
}

