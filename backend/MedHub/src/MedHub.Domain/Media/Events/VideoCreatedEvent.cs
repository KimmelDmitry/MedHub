using MedHub.Domain.Abstractions;

namespace MedHub.Domain.Media.Events;


// Событие: Видео создано и ожидает обработки
public sealed record VideoCreatedEvent(Guid VideoId, Guid LessonId) : IDomainEvent;