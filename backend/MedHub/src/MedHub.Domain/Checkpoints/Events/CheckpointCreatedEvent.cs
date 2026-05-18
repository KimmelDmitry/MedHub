using MedHub.Domain.Abstractions;

namespace MedHub.Domain.Checkpoints.Events;

public sealed record CheckpointCreatedEvent(
    Guid CheckpointId,
    Guid VideoId
) : IDomainEvent;