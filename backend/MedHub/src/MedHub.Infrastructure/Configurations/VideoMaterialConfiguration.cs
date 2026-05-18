
using MedHub.Domain.Media;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedHub.Infrastructure.Persistence.Configurations;

internal sealed class VideoConfiguration : IEntityTypeConfiguration<VideoMaterial>
{
    public void Configure(EntityTypeBuilder<VideoMaterial> builder)
    {
        builder.ToTable("videos");

        builder.HasKey(v => v.Id);

        builder.Property(v => v.Id)
            .HasColumnName("id");

        builder.Property(v => v.LessonId)
            .HasColumnName("lesson_id")
            .IsRequired();

        builder.OwnsOne(v => v.Title, title =>
        {
            title.Property(t => t.Value)
                .HasColumnName("title")
                .HasMaxLength(255)
                .IsRequired();
        });

        builder.Property(v => v.OriginalFileName)
            .HasColumnName("original_file_name")
            .HasMaxLength(255);

        builder.Property(v => v.StorageKey)
            .HasColumnName("storage_key")
            .HasMaxLength(500);

        builder.Property(v => v.UploadId)
            .HasColumnName("upload_id")
            .HasMaxLength(255);

        builder.Property(v => v.SizeBytes)
            .HasColumnName("size_bytes");

        builder.Property(v => v.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(v => v.DurationSeconds)
            .HasColumnName("duration_seconds");

        builder.Property(v => v.Width)
            .HasColumnName("width");

        builder.Property(v => v.Height)
            .HasColumnName("height");

        builder.Property(v => v.ErrorMessage)
            .HasColumnName("error_message")
            .HasMaxLength(1000);

        builder.Property(v => v.RetryCount)
            .HasColumnName("retry_count");

        builder.Property(v => v.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(v => v.UpdatedAt)
            .HasColumnName("updated_at");

       
        builder.HasIndex(v => v.LessonId);
        builder.HasIndex(v => v.Status);
    }
}
