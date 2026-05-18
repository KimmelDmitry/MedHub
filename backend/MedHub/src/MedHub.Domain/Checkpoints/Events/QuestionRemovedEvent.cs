using MedHub.Domain.Abstractions;

namespace MedHub.Domain.Checkpoints.Events;

public sealed record QuestionRemovedEvent(
    Guid CheckpointId,
    Guid QuestionId
) : IDomainEvent;