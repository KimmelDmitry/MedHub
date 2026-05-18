using FluentValidation;

namespace MedHub.Application.Checkpoints.PublishCheckpoint;

internal sealed class PublishCheckpointCommandValidator
    : AbstractValidator<PublishCheckpointCommand>
{
    public PublishCheckpointCommandValidator()
    {
        RuleFor(x => x.CheckpointId)
            .NotEmpty();
    }
}