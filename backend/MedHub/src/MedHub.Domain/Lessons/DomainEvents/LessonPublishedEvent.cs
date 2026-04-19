using MedHub.Domain.Abstractions;

namespace  MedHub.Domain.Lessons.DomainEvents;

public sealed record LessonPublishedEvent(Guid LessonId, Guid CourseId) : IDomainEvent;