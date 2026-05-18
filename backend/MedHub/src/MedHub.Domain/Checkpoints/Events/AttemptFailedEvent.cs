using MedHub.Domain.Abstractions;

namespace MedHub.Domain.Checkpoints.Events;

public sealed record AttemptFailedEvent(
    Guid AttemptId,
    string Reason
) : IDomainEvent;