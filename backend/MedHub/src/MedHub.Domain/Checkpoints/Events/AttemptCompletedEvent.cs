using MedHub.Domain.Abstractions;

namespace MedHub.Domain.Checkpoints.Events;

public sealed record AttemptCompletedEvent(
    Guid AttemptId,
    decimal Score
) : IDomainEvent;