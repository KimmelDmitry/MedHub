using MedHub.Application.Abstractions.Messaging;

namespace MedHub.Application.Checkpoints.CreateCheckpoint;

public sealed record CreateCheckpointCommand(
    Guid VideoId,
    int TimestampSeconds,
    int OrderNumber,
    string? Title,
    bool IsRequired,
    bool IsGraded
) : ICommand<Guid>;