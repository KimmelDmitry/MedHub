using MedHub.Domain.Abstractions;

namespace  MedHub.Domain.Lessons.DomainEvents;

public sealed record LessonArchivedEvent(Guid LessonId, Guid CourseId) : IDomainEvent;