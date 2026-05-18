using MedHub.Domain.Abstractions;

namespace MedHub.Domain.Checkpoints.Events;

public sealed record AttemptAnswerSubmittedEvent(
    Guid AttemptId,
    Guid QuestionId
) : IDomainEvent;