using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pivot.Framework.Infrastructure.Abstraction.EventStore.Coordinator;

namespace Pivot.Framework.Infrastructure.Persistence.EntityFrameworkCore.Configuration;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : EF Core entity configuration for <see cref="ProjectionRegistration"/>.
///              Configures the projection registration table used by the Projection Coordinator.
/// </summary>
public sealed class ProjectionRegistrationConfiguration : IEntityTypeConfiguration<ProjectionRegistration>
{
	/// <inheritdoc />
	public void Configure(EntityTypeBuilder<ProjectionRegistration> builder)
	{
		builder.ToTable("ProjectionRegistrations");

		builder.HasKey(r => r.Id);

		builder.Property(r => r.ProjectionName)
			.HasMaxLength(256)
			.IsRequired();

		builder.Property(r => r.ActiveVersion)
			.IsRequired();

		builder.Property(r => r.State)
			.IsRequired()
			.HasConversion<int>();

		builder.HasIndex(r => r.ProjectionName)
			.HasDatabaseName("IX_ProjectionRegistrations_Name")
			.IsUnique();
	}
}
