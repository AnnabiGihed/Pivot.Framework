using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pivot.Framework.Infrastructure.Abstraction.Sagas.Models;

namespace Pivot.Framework.Infrastructure.Persistence.EntityFrameworkCore.Configuration;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : EF Core entity configuration for <see cref="SagaInstance"/> and
///              <see cref="SagaStepRecord"/>.
///              Configures table mappings, indexes, relationships, and concurrency tokens
///              for the saga persistence model.
///
///              Applications must apply these configurations in their DbContext's
///              <c>OnModelCreating</c> or via <c>modelBuilder.ApplyConfigurationsFromAssembly</c>.
/// </summary>
public sealed class SagaInstanceConfiguration : IEntityTypeConfiguration<SagaInstance>
{
	/// <inheritdoc />
	public void Configure(EntityTypeBuilder<SagaInstance> builder)
	{
		builder.ToTable("SagaInstances");

		builder.HasKey(s => s.Id);

		builder.Property(s => s.SagaType)
			.HasMaxLength(256)
			.IsRequired();

		builder.Property(s => s.State)
			.IsRequired()
			.HasConversion<int>();

		builder.Property(s => s.CurrentStepIndex)
			.IsRequired();

		builder.Property(s => s.SerializedData)
			.HasColumnType("nvarchar(max)");

		builder.Property(s => s.CorrelationId)
			.HasMaxLength(128);

		builder.Property(s => s.FailureReason)
			.HasMaxLength(2048);

		// Optimistic concurrency — EF Core will include Version in the WHERE clause
		// of UPDATE statements, detecting conflicting modifications.
		builder.Property(s => s.Version)
			.IsConcurrencyToken();

		// Index for querying active sagas by type
		builder.HasIndex(s => new { s.SagaType, s.State })
			.HasDatabaseName("IX_SagaInstances_SagaType_State");
	}
}

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : EF Core entity configuration for <see cref="SagaStepRecord"/>.
/// </summary>
public sealed class SagaStepRecordConfiguration : IEntityTypeConfiguration<SagaStepRecord>
{
	/// <inheritdoc />
	public void Configure(EntityTypeBuilder<SagaStepRecord> builder)
	{
		builder.ToTable("SagaStepRecords");

		builder.HasKey(r => r.Id);

		builder.Property(r => r.SagaInstanceId)
			.IsRequired();

		builder.Property(r => r.StepName)
			.HasMaxLength(256)
			.IsRequired();

		builder.Property(r => r.StepIndex)
			.IsRequired();

		builder.Property(r => r.Status)
			.IsRequired()
			.HasConversion<int>();

		builder.Property(r => r.Error)
			.HasMaxLength(2048);

		// Foreign key relationship to SagaInstance
		builder.HasIndex(r => r.SagaInstanceId)
			.HasDatabaseName("IX_SagaStepRecords_SagaInstanceId");
	}
}
