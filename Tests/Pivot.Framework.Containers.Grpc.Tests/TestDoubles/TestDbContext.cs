using Microsoft.EntityFrameworkCore;
using Pivot.Framework.Infrastructure.Abstraction.Persistence;

namespace Pivot.Framework.Containers.Grpc.Tests.TestDoubles;

/// <summary>
/// In-memory DbContext used for gRPC transaction tests.
/// </summary>
public sealed class TestDbContext : DbContext, IPersistenceContext
{
	#region Constructor
	/// <summary>
	/// Initializes a new instance of <see cref="TestDbContext"/>.
	/// </summary>
	public TestDbContext()
		: base(new DbContextOptionsBuilder<TestDbContext>()
			.UseInMemoryDatabase(Guid.NewGuid().ToString())
			.Options)
	{
	}
	#endregion
}
