using FluentValidation;

namespace MedHub.Application.Checkpoints.CreateCheckpoint;

internal sealed class CreateCheckpointCommandValidator
    : AbstractValidator<CreateCheckpointCommand>
{
    public CreateCheckpointCommandValidator()
    {
        RuleFor(x => x.VideoId)
            .NotEmpty();

        RuleFor(x => x.TimestampSeconds)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.OrderNumber)
            .GreaterThan(0);

        RuleFor(x => x.Title)
            .MaximumLength(300);
    }
}