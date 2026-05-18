using MedHub.Domain.Abstractions;

namespace MedHub.Domain.Lessons.DomainEvents;

public sealed record LessonVideoAttachedEvent(Guid LessonId, Guid VideoId) : IDomainEvent;