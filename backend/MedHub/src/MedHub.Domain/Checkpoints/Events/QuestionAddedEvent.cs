using MedHub.Domain.Abstractions;

namespace MedHub.Domain.Checkpoints.Events;

public sealed record QuestionAddedEvent(
    Guid CheckpointId,
    Guid QuestionId
) : IDomainEvent;