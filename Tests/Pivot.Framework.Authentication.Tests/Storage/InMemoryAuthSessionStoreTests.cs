using FluentAssertions;
using Pivot.Framework.Authentication.Models;
using Pivot.Framework.Authentication.Storage;

namespace Pivot.Framework.Authentication.Tests.Storage;

public class InMemoryAuthSessionStoreTests
{
	[Fact]
	public async Task SaveGetRemoveAsync_ShouldRoundTripSession()
	{
		var store = new InMemoryAuthSessionStore();
		var session = new AuthSession
		{
			SessionId = "session-1",
			SubjectId = "user-1",
			AccessToken = "access",
			RefreshToken = "refresh",
			ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(5)
		};

		await store.SaveAsync(session);
		var stored = await store.GetAsync("session-1");
		await store.RemoveAsync("session-1");
		var removed = await store.GetAsync("session-1");

		stored.Should().NotBeNull();
		stored!.SubjectId.Should().Be("user-1");
		removed.Should().BeNull();
	}
}
