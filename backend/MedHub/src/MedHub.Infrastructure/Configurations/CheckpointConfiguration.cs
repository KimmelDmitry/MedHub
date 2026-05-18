using MedHub.Domain.Checkpoints;
using MedHub.Domain.Checkpoints.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedHub.Infrastructure.Configurations;

internal sealed class CheckpointConfiguration : IEntityTypeConfiguration<Checkpoint>
{
    public void Configure(EntityTypeBuilder<Checkpoint> builder)
    {
        builder.ToTable("checkpoints");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.VideoId)
            .HasColumnName("video_id")
            .IsRequired();

        builder.Property(x => x.Timestamp)
            .HasColumnName("timestamp_seconds")
            .HasConversion(
                v => v.Value,
                v => CheckpointTimestamp.Create(v, int.MaxValue).Value)
            .IsRequired();

        builder.Property(x => x.OrderNumber)
            .HasColumnName("order_number")
            .IsRequired();

        builder.Property(x => x.Title)
            .HasColumnName("title")
            .HasMaxLength(300);

        builder.Property(x => x.IsRequired)
            .HasColumnName("is_required")
            .IsRequired();

        builder.Property(x => x.IsGraded)
            .HasColumnName("is_graded")
            .IsRequired();

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at");

        builder.HasIndex(x => x.VideoId);
        builder.HasIndex(x => new { x.VideoId, x.Timestamp }).IsUnique();
        builder.HasIndex(x => new { x.VideoId, x.OrderNumber }).IsUnique();

        builder.HasMany(x => x.Questions)
            .WithOne()
            .HasForeignKey(x => x.CheckpointId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}