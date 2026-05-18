using MedHub.Domain.Abstractions;

namespace MedHub.Domain.Checkpoints.Events;

public sealed record AttemptStartedEvent(
    Guid AttemptId,
    Guid StudentId,
    Guid LessonId
) : IDomainEvent;