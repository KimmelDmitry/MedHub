using MedHub.Infrastructure.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedHub.Infrastructure.Configurations;

internal sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("outbox_messages");

        builder.HasKey(outboxMessage => outboxMessage.Id);

        builder.Property(outboxMessage => outboxMessage.Content).HasColumnType("jsonb");
    }
}
