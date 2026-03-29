using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pivot.Framework.Infrastructure.Abstraction.EventStore.Models;
using Pivot.Framework.Infrastructure.Persistence.EntityFrameworkCore.Configuration;

namespace Pivot.Framework.Infrastructure.Persistence.PostgreSQL.Configuration;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : PostgreSQL-specific EF Core configuration for <see cref="EventHistoryEntry"/>.
///              Overrides the base SQL Server configuration to use JSONB for the Payload column
///              and timestamptz for UTC date columns.
/// </summary>
public sealed class PostgreSqlEventHistoryEntryConfiguration : IEntityTypeConfiguration<EventHistoryEntry>
{
	/// <inheritdoc />
	public void Configure(EntityTypeBuilder<EventHistoryEntry> builder)
	{
		// Apply base configuration first
		new EventHistoryEntryConfiguration().Configure(builder);

		// Override Payload to use PostgreSQL JSONB
		builder.Property(e => e.Payload)
			.HasColumnType("jsonb");

		// Override date columns to use timestamptz
		builder.Property(e => e.OccurredOnUtc)
			.HasColumnType("timestamptz");

		builder.Property(e => e.CreatedAtUtc)
			.HasColumnType("timestamptz");
	}
}
