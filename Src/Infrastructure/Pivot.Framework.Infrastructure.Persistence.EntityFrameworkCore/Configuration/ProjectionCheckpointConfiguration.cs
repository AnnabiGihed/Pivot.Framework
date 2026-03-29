using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pivot.Framework.Infrastructure.Abstraction.EventStore.Models;

namespace Pivot.Framework.Infrastructure.Persistence.EntityFrameworkCore.Configuration;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : EF Core entity configuration for <see cref="ProjectionCheckpoint"/>.
///              Configures the projection checkpoint table used for tracking
///              projection rebuild/catch-up progress.
/// </summary>
public sealed class ProjectionCheckpointConfiguration : IEntityTypeConfiguration<ProjectionCheckpoint>
{
	/// <inheritdoc />
	public void Configure(EntityTypeBuilder<ProjectionCheckpoint> builder)
	{
		builder.ToTable("ProjectionCheckpoints");

		builder.HasKey(c => c.Id);

		builder.Property(c => c.ProjectionName)
			.HasMaxLength(256)
			.IsRequired();

		builder.Property(c => c.ProjectionVersion)
			.IsRequired();

		builder.Property(c => c.LastProcessedPosition)
			.IsRequired();

		builder.Property(c => c.LastUpdatedUtc)
			.IsRequired();

		// Unique index on projection name + version
		builder.HasIndex(c => new { c.ProjectionName, c.ProjectionVersion })
			.HasDatabaseName("IX_ProjectionCheckpoints_Name_Version")
			.IsUnique();
	}
}
