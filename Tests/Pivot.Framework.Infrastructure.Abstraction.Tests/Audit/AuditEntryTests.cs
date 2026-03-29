using FluentAssertions;
using Pivot.Framework.Infrastructure.Abstraction.Audit;

namespace Pivot.Framework.Infrastructure.Abstraction.Tests.Audit;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Unit tests for <see cref="AuditEntry"/> and <see cref="AuditQuery"/>.
///              Verifies default property values and property assignment for audit logging models.
/// </summary>
public class AuditEntryTests
{
	#region AuditEntry Tests

	[Fact]
	public void AuditEntry_ShouldHaveDefaultId()
	{
		var entry = new AuditEntry
		{
			ActorId = "user-1",
			Action = "Activate",
			ResourceType = "ServiceClient"
		};

		entry.Id.Should().NotBe(Guid.Empty);
	}

	[Fact]
	public void AuditEntry_ShouldHaveDefaultOccurredAtUtc()
	{
		var before = DateTime.UtcNow;
		var entry = new AuditEntry
		{
			ActorId = "user-1",
			Action = "Deactivate",
			ResourceType = "Projection"
		};
		var after = DateTime.UtcNow;

		entry.OccurredAtUtc.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
	}

	[Fact]
	public void AuditEntry_ShouldSetAllProperties()
	{
		var entry = new AuditEntry
		{
			Id = Guid.Parse("aaaa1111-2222-3333-4444-555566667777"),
			ActorId = "admin-42",
			ActorName = "Jane Admin",
			Action = "ReplayStart",
			ResourceType = "EventStore",
			ResourceId = "projection-v3",
			Details = "Started replay from position 0",
			CorrelationId = "corr-123",
			IpAddress = "10.0.0.1",
			OccurredAtUtc = new DateTime(2026, 3, 1, 12, 0, 0, DateTimeKind.Utc)
		};

		entry.ActorId.Should().Be("admin-42");
		entry.ActorName.Should().Be("Jane Admin");
		entry.Action.Should().Be("ReplayStart");
		entry.ResourceType.Should().Be("EventStore");
		entry.ResourceId.Should().Be("projection-v3");
		entry.Details.Should().Be("Started replay from position 0");
		entry.CorrelationId.Should().Be("corr-123");
		entry.IpAddress.Should().Be("10.0.0.1");
	}

	#endregion

	#region AuditQuery Tests

	[Fact]
	public void AuditQuery_ShouldHaveDefaultTake()
	{
		var query = new AuditQuery();

		query.Take.Should().Be(50);
		query.Skip.Should().Be(0);
	}

	[Fact]
	public void AuditQuery_ShouldSetFilterProperties()
	{
		var query = new AuditQuery
		{
			ActorId = "user-1",
			Action = "Delete",
			ResourceType = "Record",
			ResourceId = "rec-42",
			FromUtc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
			ToUtc = new DateTime(2026, 12, 31, 23, 59, 59, DateTimeKind.Utc),
			Skip = 10,
			Take = 25
		};

		query.ActorId.Should().Be("user-1");
		query.Action.Should().Be("Delete");
		query.ResourceType.Should().Be("Record");
		query.ResourceId.Should().Be("rec-42");
		query.FromUtc.Should().NotBeNull();
		query.ToUtc.Should().NotBeNull();
		query.Skip.Should().Be(10);
		query.Take.Should().Be(25);
	}

	#endregion
}
