using MedHub.Domain.Abstractions;

namespace MedHub.Domain.Media.Events;


// Событие: Обработка провалилась
public sealed record VideoProcessingFailedEvent(Guid VideoId, Guid LessonId, string Reason) : IDomainEvent;