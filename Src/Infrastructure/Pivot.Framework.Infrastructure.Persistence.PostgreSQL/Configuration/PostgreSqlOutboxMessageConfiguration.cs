using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pivot.Framework.Infrastructure.Abstraction.Outbox.Models;

namespace Pivot.Framework.Infrastructure.Persistence.PostgreSQL.Configuration;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : PostgreSQL-specific EF Core configuration for <see cref="OutboxMessage"/>.
///              Overrides column types to use JSONB for payload and timestamptz for UTC dates.
/// </summary>
public sealed class PostgreSqlOutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
	/// <inheritdoc />
	public void Configure(EntityTypeBuilder<OutboxMessage> builder)
	{
		// Override Payload to use PostgreSQL JSONB
		builder.Property(m => m.Payload)
			.HasColumnType("jsonb");

		// Override date columns to use timestamptz
		builder.Property(m => m.CreatedAtUtc)
			.HasColumnType("timestamptz");

		builder.Property(m => m.ProcessedAtUtc)
			.HasColumnType("timestamptz");

		builder.Property(m => m.FailedAtUtc)
			.HasColumnType("timestamptz");
	}
}
