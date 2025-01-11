using System.Reflection;

namespace Templates.Core.Infrastructure.Persistence.EntityFrameworkCore;

public static class AssemblyReference
{
	public static readonly Assembly Assembly = typeof(AssemblyReference).Assembly;
}
