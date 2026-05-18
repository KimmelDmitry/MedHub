using FluentValidation;

namespace MedHub.Application.Checkpoints.UpdateCheckpoint;

internal sealed class UpdateCheckpointCommandValidator
    : AbstractValidator<UpdateCheckpointCommand>
{
    public UpdateCheckpointCommandValidator()
    {
        RuleFor(x => x.CheckpointId)
            .NotEmpty();

        RuleFor(x => x.Title)
            .MaximumLength(300);

        RuleFor(x => x)
            .Must(x =>
                x.Title is not null ||
                x.TimestampSeconds is not null ||
                x.OrderNumber is not null ||
                x.IsRequired is not null ||
                x.IsGraded is not null)
            .WithMessage("Хоть одно поле должно быть передано для обновления.");

        RuleFor(x => x.TimestampSeconds)
            .GreaterThanOrEqualTo(0)
            .When(x => x.TimestampSeconds.HasValue);

        RuleFor(x => x.OrderNumber)
            .GreaterThan(0)
            .When(x => x.OrderNumber.HasValue);
    }
}