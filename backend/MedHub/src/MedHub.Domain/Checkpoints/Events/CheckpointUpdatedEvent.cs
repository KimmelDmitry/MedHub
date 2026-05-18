using MedHub.Domain.Abstractions;

namespace MedHub.Domain.Checkpoints.Events;

public sealed record CheckpointUpdatedEvent(
    Guid CheckpointId
) : IDomainEvent;