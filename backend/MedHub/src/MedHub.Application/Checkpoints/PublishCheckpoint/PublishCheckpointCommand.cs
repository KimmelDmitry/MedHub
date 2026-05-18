using MedHub.Application.Abstractions.Messaging;

namespace MedHub.Application.Checkpoints.PublishCheckpoint;

public sealed record PublishCheckpointCommand(
    Guid CheckpointId
) : ICommand;