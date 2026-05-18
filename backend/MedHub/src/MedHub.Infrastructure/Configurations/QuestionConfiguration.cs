using MedHub.Domain.Checkpoints;
using MedHub.Domain.Checkpoints.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedHub.Infrastructure.Configurations;

internal sealed class QuestionConfiguration : IEntityTypeConfiguration<Question>
{
    public void Configure(EntityTypeBuilder<Question> builder)
    {
        builder.ToTable("checkpoint_questions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.CheckpointId)
            .HasColumnName("checkpoint_id")
            .IsRequired();

        builder.Property(x => x.Text)
            .HasColumnName("text")
            .HasMaxLength(1000)
            .HasConversion(
                v => v.Value,
                v => QuestionText.Create(v).Value)
            .IsRequired();

        builder.Property(x => x.Type)
            .HasColumnName("type")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.AllowRetry)
            .HasColumnName("allow_retry")
            .IsRequired();

        builder.Property(x => x.TimeLimitSeconds)
            .HasColumnName("time_limit_seconds");

        builder.Property(x => x.RevealCorrectAnswer)
            .HasColumnName("reveal_correct_answer")
            .IsRequired();

        builder.Property(x => x.CorrectTextAnswer)
            .HasColumnName("correct_text_answer")
            .HasMaxLength(1000);

        builder.HasIndex(x => x.CheckpointId);
        builder.HasIndex(x => new { x.CheckpointId, x.Type });

        builder.HasMany(x => x.AnswerOptions)
            .WithOne()
            .HasForeignKey(x => x.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}