namespace MedHub.Domain.Media;

public enum VideoStatus
{
    /// <summary>
    /// Файл загружается или ожидает начала обработки.
    /// </summary>
    Uploading = 1,
    
    /// <summary>
    ///  Файл загрузился
    /// </summary>
    Uploaded = 2,

    /// <summary>
    /// Видео отправлено в очередь обработки (Quartz Job).
    /// Идет транскодинг, извлечение метаданных.
    /// </summary>
    Processing = 3,

    /// <summary>
    /// Видео готово к воспроизведению. Метаданные (длительность) заполнены.
    /// Можно создавать чекпоинты.
    /// </summary>
    Ready = 4,

    /// <summary>
    /// Обработка завершилась ошибкой.
    /// </summary>
    Failed = 5
}