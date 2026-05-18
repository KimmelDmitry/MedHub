using MedHub.Domain.Checkpoints;
using MedHub.Domain.Checkpoints.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedHub.Infrastructure.Configurations;

internal sealed class AnswerOptionConfiguration : IEntityTypeConfiguration<AnswerOption>
{
    public void Configure(EntityTypeBuilder<AnswerOption> builder)
    {
        builder.ToTable("checkpoint_answer_options");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.QuestionId)
            .HasColumnName("question_id")
            .IsRequired();

        builder.Property(x => x.Text)
            .HasColumnName("text")
            .HasMaxLength(1000)
            .HasConversion(
                v => v.Value,
                v => AnswerOptionText.Create(v).Value)
            .IsRequired();

        builder.Property(x => x.IsCorrect)
            .HasColumnName("is_correct")
            .IsRequired();

        builder.HasIndex(x => x.QuestionId);
        builder.HasIndex(x => new { x.QuestionId, x.IsCorrect });
    }
}