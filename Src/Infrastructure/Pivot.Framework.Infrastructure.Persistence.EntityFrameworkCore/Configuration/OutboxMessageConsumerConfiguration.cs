using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pivot.Framework.Infrastructure.Abstraction.Outbox.Models;

namespace Pivot.Framework.Infrastructure.Persistence.EntityFrameworkCore.Configuration;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : EF Core entity configuration for <see cref="OutboxMessageConsumer"/>.
///              Configures the composite primary key (MessageId + ConsumerName) used by the
///              inbox pattern for consumer-side idempotent message deduplication.
///
///              Applications must apply this configuration in their DbContext's
///              <c>OnModelCreating</c> or via <c>modelBuilder.ApplyConfigurationsFromAssembly</c>.
/// </summary>
public sealed class OutboxMessageConsumerConfiguration : IEntityTypeConfiguration<OutboxMessageConsumer>
{
	/// <inheritdoc />
	public void Configure(EntityTypeBuilder<OutboxMessageConsumer> builder)
	{
		builder.ToTable("OutboxMessageConsumers");

		// Composite key: a message can be processed by multiple consumers,
		// but each consumer can only process a given message once.
		builder.HasKey(c => new { c.Id, c.Name });

		builder.Property(c => c.Id)
			.IsRequired();

		builder.Property(c => c.Name)
			.HasMaxLength(512)
			.IsRequired();
	}
}
