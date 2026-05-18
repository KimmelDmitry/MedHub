using MedHub.Domain.Abstractions;

namespace MedHub.Domain.Checkpoints.Events;

public sealed record AnswerOptionRemovedEvent(
    Guid QuestionId,
    Guid AnswerOptionId
) : IDomainEvent;