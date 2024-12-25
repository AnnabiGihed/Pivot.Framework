using System.Reflection;

namespace Templates.Core.Tools.API;

public static class AssemblyReference
{
	public static readonly Assembly Assembly = typeof(AssemblyReference).Assembly;
}
