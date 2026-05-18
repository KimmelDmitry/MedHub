using MedHub.Domain.Abstractions;

namespace MedHub.Domain.Checkpoints.Events;

public sealed record AnswerOptionAddedEvent(
    Guid QuestionId,
    Guid AnswerOptionId
) : IDomainEvent;