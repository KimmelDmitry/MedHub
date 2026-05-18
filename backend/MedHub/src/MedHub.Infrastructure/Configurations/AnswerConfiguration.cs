using MedHub.Domain.Checkpoints;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedHub.Infrastructure.Configurations;

internal sealed class AnswerConfiguration : IEntityTypeConfiguration<Answer>
{
    public void Configure(EntityTypeBuilder<Answer> builder)
    {
        builder.ToTable("attempt_answers");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.AttemptId)
            .HasColumnName("attempt_id")
            .IsRequired();

        builder.Property(x => x.QuestionId)
            .HasColumnName("question_id")
            .IsRequired();
        
        builder.Ignore(x => x.SelectedOptionIds);

        builder.Property<List<Guid>>("_selectedOptionIds")
            .HasColumnName("selected_option_ids")
            .HasColumnType("uuid[]")
            .IsRequired();
        
        

        builder.Property(x => x.TextAnswer)
            .HasColumnName("text_answer")
            .HasMaxLength(2000);

        builder.Property(x => x.IsCorrect)
            .HasColumnName("is_correct")
            .IsRequired();

        builder.Property(x => x.RequiresManualReview)
            .HasColumnName("requires_manual_review")
            .IsRequired();

        builder.Property(x => x.AnsweredAt)
            .HasColumnName("answered_at")
            .IsRequired();

    }
}