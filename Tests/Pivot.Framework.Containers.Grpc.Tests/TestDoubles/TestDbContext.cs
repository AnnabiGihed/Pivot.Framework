using Microsoft.EntityFrameworkCore;
using Pivot.Framework.Infrastructure.Abstraction.Persistence;

namespace Pivot.Framework.Containers.Grpc.Tests.TestDoubles;

public sealed class TestDbContext : DbContext, IPersistenceContext
{
	public TestDbContext()
		: base(new DbContextOptionsBuilder<TestDbContext>()
			.UseInMemoryDatabase(Guid.NewGuid().ToString())
			.Options)
	{
	}
}
