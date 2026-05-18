using MedHub.Domain.Courses;
using MedHub.Domain.Courses.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedHub.Infrastructure.Configurations;

internal sealed class CourseConfiguration : IEntityTypeConfiguration<Course>
{
    public void Configure(EntityTypeBuilder<Course> builder)
    {
        builder.ToTable("courses");

        builder.HasKey(c => c.Id);

        // Value Object: Title
        builder.Property(c => c.Title)
            .HasColumnName("title")
            .HasMaxLength(200)
            .HasConversion(
                v => v.Value,
                v => CourseTitle.Create(v).Value
            )
            .IsRequired();

        // Value Object: Description
        builder.Property(c => c.Description)
            .HasColumnName("description")
            .HasMaxLength(2000)
            .HasConversion(
                v => v.Value,
                v => CourseDescription.Create(v).Value
            );

        builder.Property(c => c.CreatorId)
            .HasColumnName("creator_id")
            .IsRequired();

        builder.Property(c => c.Status)
            .HasColumnName("status")
            .HasConversion<string>() // Enum ("Draft", "Published")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(c => c.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.HasMany(c => c.Lessons)
            .WithOne() 
            .HasForeignKey(l => l.CourseId)
            .OnDelete(DeleteBehavior.Cascade); 

        builder.HasIndex(c => c.CreatorId);
        builder.HasIndex(c => c.Status);
    }
}