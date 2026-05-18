using MedHub.Domain.Abstractions;

namespace MedHub.Domain.Media.Events;

// Событие: Обработка завершена успешно, метаданные получены
// здесь передаем Duration, чтобы другие части системы знали длину видео
public sealed record VideoProcessingCompletedEvent(Guid VideoId, Guid LessonId, int DurationSeconds) : IDomainEvent;
