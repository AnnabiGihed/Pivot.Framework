using System.Reflection;

namespace Templates.Core.Infrastructure.SharedServices;

public static class AssemblyReference
{
	public static readonly Assembly Assembly = typeof(AssemblyReference).Assembly;
}
