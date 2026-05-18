using FluentValidation;

namespace MedHub.Application.Checkpoints.DeleteCheckpoint;

internal sealed class DeleteCheckpointCommandValidator
    : AbstractValidator<DeleteCheckpointCommand>
{
    public DeleteCheckpointCommandValidator()
    {
        RuleFor(x => x.CheckpointId)
            .NotEmpty();
    }
}