using FluentValidation;

namespace MedHub.Application.Checkpoints.ArchiveCheckpoint;

internal sealed class ArchiveCheckpointCommandValidator
    : AbstractValidator<ArchiveCheckpointCommand>
{
    public ArchiveCheckpointCommandValidator()
    {
        RuleFor(x => x.CheckpointId)
            .NotEmpty();
    }
}