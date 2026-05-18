using MedHub.Application.Abstractions.Messaging;

namespace MedHub.Application.Checkpoints.ArchiveCheckpoint;

public sealed record ArchiveCheckpointCommand(
    Guid CheckpointId
) : ICommand;