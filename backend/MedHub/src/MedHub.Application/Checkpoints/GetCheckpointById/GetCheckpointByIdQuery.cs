using MedHub.Application.Abstractions.Messaging;

namespace MedHub.Application.Checkpoints.GetCheckpointById;

public sealed record GetCheckpointByIdQuery(
    Guid CheckpointId
) : IQuery<CheckpointResponse>;