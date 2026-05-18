using MedHub.Application.Abstractions.Messaging;

namespace MedHub.Application.Checkpoints.DeleteCheckpoint;

public sealed record DeleteCheckpointCommand(
    Guid CheckpointId
) : ICommand;