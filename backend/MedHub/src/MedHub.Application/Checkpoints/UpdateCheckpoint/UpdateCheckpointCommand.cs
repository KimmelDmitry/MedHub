using MedHub.Application.Abstractions.Messaging;

namespace MedHub.Application.Checkpoints.UpdateCheckpoint;

public sealed record UpdateCheckpointCommand(
    Guid CheckpointId,
    string? Title,
    int? TimestampSeconds,
    int? OrderNumber,
    bool? IsRequired,
    bool? IsGraded
) : ICommand;