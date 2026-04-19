using MedHub.Domain.Abstractions;

namespace  MedHub.Domain.Lessons.DomainEvents;

public sealed record LessonCreatedEvent(Guid LessonId, Guid CourseId) : IDomainEvent;