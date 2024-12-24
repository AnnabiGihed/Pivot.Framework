using System.Reflection;

namespace Templates.Core.Infrastructure.Messaging.EntityFrameworkCore;

public static class AssemblyReference
{
	public static readonly Assembly Assembly = typeof(AssemblyReference).Assembly;
}
