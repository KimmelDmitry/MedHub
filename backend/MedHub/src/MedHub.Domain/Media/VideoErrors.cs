using MedHub.Domain.Abstractions;

namespace MedHub.Domain.Media;

public static class VideoErrors
{
    public static Error NotFound => new("Video.NotFound", "Видео не найдено");

    public static Error InvalidStatusTransition => new(
        "Video.InvalidStatusTransition", 
        "Недопустимый переход статуса видео");

    public static Error MetadataNotAvailable => new(
        "Video.MetadataNotAvailable", 
        "Метаданные видео еще не доступны (обработка не завершена)");

    public static Error CheckpointOutOfRange => new(
        "Video.CheckpointOutOfRange", 
        "Время чекпоинта выходит за пределы длительности видео");

    public static Error AlreadyProcessed => new(
        "Video.AlreadyProcessed", 
        "Видео уже обработано и готово к использованию");
}