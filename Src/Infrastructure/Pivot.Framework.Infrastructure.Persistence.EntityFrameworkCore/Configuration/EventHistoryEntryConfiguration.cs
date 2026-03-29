using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pivot.Framework.Infrastructure.Abstraction.EventStore.Models;

namespace Pivot.Framework.Infrastructure.Persistence.EntityFrameworkCore.Configuration;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : EF Core entity configuration for <see cref="EventHistoryEntry"/>.
///              Configures the append-only event store table with appropriate indexes
///              for aggregate stream queries and global position-based reads.
///
///              The Payload column uses nvarchar(max) on SQL Server.
///              For PostgreSQL, the <c>PostgreSqlEventHistoryEntryConfiguration</c> overrides
///              this to use JSONB.
/// </summary>
public class EventHistoryEntryConfiguration : IEntityTypeConfiguration<EventHistoryEntry>
{
	/// <inheritdoc />
	public virtual void Configure(EntityTypeBuilder<EventHistoryEntry> builder)
	{
		builder.ToTable("EventHistory");

		builder.HasKey(e => e.Id);

		builder.Property(e => e.EventType)
			.HasMaxLength(512)
			.IsRequired();

		builder.Property(e => e.EventVersion)
			.IsRequired();

		builder.Property(e => e.OccurredOnUtc)
			.IsRequired();

		builder.Property(e => e.ProducerService)
			.HasMaxLength(256)
			.IsRequired();

		builder.Property(e => e.CorrelationId)
			.HasMaxLength(128);

		builder.Property(e => e.CausationId)
			.HasMaxLength(128);

		builder.Property(e => e.AggregateType)
			.HasMaxLength(256);

		builder.Property(e => e.AggregateId)
			.HasMaxLength(256);

		builder.Property(e => e.AggregateVersion)
			.IsRequired();

		builder.Property(e => e.ReplayFlag)
			.IsRequired();

		builder.Property(e => e.Payload)
			.HasColumnType("nvarchar(max)")
			.IsRequired();

		builder.Property(e => e.CreatedAtUtc)
			.IsRequired();

		// Index for aggregate stream queries (GetByAggregateId)
		builder.HasIndex(e => new { e.AggregateId, e.AggregateType, e.AggregateVersion })
			.HasDatabaseName("IX_EventHistory_Aggregate")
			.IsUnique();

		// Index for global position-based reads (projection catch-up)
		builder.HasIndex(e => new { e.CreatedAtUtc, e.Id })
			.HasDatabaseName("IX_EventHistory_Position");

		// Index for correlation tracing
		builder.HasIndex(e => e.CorrelationId)
			.HasDatabaseName("IX_EventHistory_CorrelationId");
	}
}
