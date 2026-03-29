using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pivot.Framework.Infrastructure.Abstraction.Audit;

namespace Pivot.Framework.Infrastructure.Persistence.EntityFrameworkCore.Configuration;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : EF Core entity configuration for <see cref="AuditEntry"/>.
///              Configures the AuditLog table for persisting administrative action audit trails.
/// </summary>
public sealed class AuditEntryConfiguration : IEntityTypeConfiguration<AuditEntry>
{
	/// <inheritdoc />
	public void Configure(EntityTypeBuilder<AuditEntry> builder)
	{
		builder.ToTable("AuditLog");

		builder.HasKey(e => e.Id);

		builder.Property(e => e.ActorId).HasMaxLength(256).IsRequired();
		builder.Property(e => e.ActorName).HasMaxLength(256);
		builder.Property(e => e.Action).HasMaxLength(256).IsRequired();
		builder.Property(e => e.ResourceType).HasMaxLength(256).IsRequired();
		builder.Property(e => e.ResourceId).HasMaxLength(256);
		builder.Property(e => e.Details).HasColumnType("nvarchar(max)");
		builder.Property(e => e.CorrelationId).HasMaxLength(128);
		builder.Property(e => e.IpAddress).HasMaxLength(64);
		builder.Property(e => e.OccurredAtUtc).IsRequired();

		builder.HasIndex(e => e.ActorId).HasDatabaseName("IX_AuditLog_ActorId");
		builder.HasIndex(e => e.OccurredAtUtc).HasDatabaseName("IX_AuditLog_OccurredAtUtc");
		builder.HasIndex(e => new { e.ResourceType, e.ResourceId }).HasDatabaseName("IX_AuditLog_Resource");
	}
}
