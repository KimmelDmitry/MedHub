using MedHub.Domain.Courses;
using MedHub.Domain.Lessons;
using MedHub.Domain.Lessons.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedHub.Infrastructure.Configurations;

internal sealed class LessonConfiguration : IEntityTypeConfiguration<Lesson>
{
    public void Configure(EntityTypeBuilder<Lesson> builder)
    {
        builder.ToTable("lessons");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Title)
            .HasColumnName("title")
            .HasMaxLength(150)
            .HasConversion(
                v => v.Value,
                v => LessonTitle.Create(v).Value 
            )
            .IsRequired();

        builder.Property(l => l.OrderNumber)
            .HasColumnName("order_number")
            .HasConversion(
                v => v.Value,
                v => LessonOrder.Create(v).Value 
            )
            .IsRequired();
        
        builder.Property(x => x.VideoId);

        // 3. Простые свойства
        builder.Property(l => l.ContentType)
            .HasColumnName("content_type")
            .HasConversion<string>() // Enum ("Video", "Text")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(l => l.ContentUrl)
            .HasColumnName("content_url")
            .HasMaxLength(500);

        builder.Property(l => l.Status)
            .HasColumnName("status")
            .HasConversion<string>() // Enum ("Draft", "Published")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(l => l.CourseId)
            .HasColumnName("course_id")
            .IsRequired();

        builder.Property(l => l.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        // 4. Настройка связи с Курсом (Many-to-One)
        
        builder.HasOne<Course>()
            .WithMany(c => c.Lessons) 
            .HasForeignKey(l => l.CourseId)
            .OnDelete(DeleteBehavior.Cascade); 

        builder.HasIndex(l => l.CourseId);
        builder.HasIndex(l => l.OrderNumber);
        
        
        builder.HasIndex(l => new { l.CourseId, l.OrderNumber }).IsUnique();
    }
}