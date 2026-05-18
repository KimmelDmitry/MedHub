using MedHub.Domain.Abstractions;
using MedHub.Domain.Media.Events;
using MedHub.Domain.Media.ValueObjects;

namespace MedHub.Domain.Media;

public sealed class VideoMaterial : Entity
{
    private VideoMaterial()
    {
    }

    private VideoMaterial(
        Guid id,
        Guid lessonId,
        VideoTitle title,
        string originalFileName)
        : base(id)
    {
        LessonId = lessonId;
        Title = title;
        OriginalFileName = originalFileName;
        Status = VideoStatus.Uploading;
        CreatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new VideoCreatedEvent(Id, LessonId));
    }

    public Guid LessonId { get; private set; }

    public VideoTitle Title { get; private set; } = null!;

    public string OriginalFileName { get; private set; } = null!;

    /// <summary>
    /// Ключ в хранилище (S3/MinIO).
    /// Uploading/Uploaded/Processing → raw файл
    /// Ready → master.m3u8
    /// </summary>
    public string? StorageKey { get; private set; }

    /// <summary>
    /// Multipart UploadId (S3/MinIO)
    /// </summary>
    public string? UploadId { get; private set; }

    /// <summary>
    /// Размер файла
    /// </summary>
    public long? SizeBytes { get; private set; }

    public VideoStatus Status { get; private set; }

    public int? DurationSeconds { get; private set; }

    public int? Width { get; private set; }

    public int? Height { get; private set; }

    public string? ErrorMessage { get; private set; }

    public int RetryCount { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime? UpdatedAt { get; private set; }

    // =========================
    // FACTORY
    // =========================

    public static Result<VideoMaterial> Create(Guid lessonId, string title, string originalFileName)
    {
        var titleResult = VideoTitle.Create(title);

        if (titleResult.IsFailure)
            return Result.Failure<VideoMaterial>(titleResult.Error);

        if (string.IsNullOrWhiteSpace(originalFileName))
            return Result.Failure<VideoMaterial>(
                new Error("Video.FileNameRequired", "Имя файла обязательно"));

        var video = new VideoMaterial(Guid.NewGuid(), lessonId, titleResult.Value, originalFileName);

        return Result.Success(video);
    }

    // =========================
    // UPLOAD FLOW
    // =========================

    public void StartUpload(string storageKey, string uploadId, long sizeBytes)
    {
        if (Status != VideoStatus.Uploading)
            throw new InvalidOperationException("Upload уже был начат");

        if (string.IsNullOrWhiteSpace(storageKey))
            throw new ArgumentException("StorageKey обязателен");

        if (string.IsNullOrWhiteSpace(uploadId))
            throw new ArgumentException("UploadId обязателен");

        StorageKey = storageKey;
        UploadId = uploadId;
        SizeBytes = sizeBytes;
        UpdatedAt = DateTime.UtcNow;
    }

    public Result MarkUploaded()
    {
        if (Status != VideoStatus.Uploading)
            return Result.Failure(VideoErrors.InvalidStatusTransition);

        Status = VideoStatus.Uploaded;
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    public Result StartProcessing()
    {
        if (Status != VideoStatus.Uploaded)
            return Result.Failure(VideoErrors.InvalidStatusTransition);

        Status = VideoStatus.Processing;
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    // =========================
    // PROCESSING FLOW
    // =========================

    public Result CompleteProcessing(int durationSeconds, int width, int height, string hlsMasterKey)
    {
        if (Status == VideoStatus.Failed)
            return Result.Failure(VideoErrors.InvalidStatusTransition);

        if (Status == VideoStatus.Ready)
            return Result.Failure(VideoErrors.AlreadyProcessed);

        if (durationSeconds <= 0)
            return Result.Failure(
                new Error("Video.InvalidDuration", "Длительность должна быть > 0"));

        if (string.IsNullOrWhiteSpace(hlsMasterKey))
            return Result.Failure(
                new Error("Video.InvalidKey", "Ключ HLS обязателен"));

        DurationSeconds = durationSeconds;
        Width = width;
        Height = height;

        StorageKey = hlsMasterKey;
        Status = VideoStatus.Ready;
        ErrorMessage = null;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(
            new VideoProcessingCompletedEvent(Id, LessonId, durationSeconds));

        return Result.Success();
    }

    public Result MarkAsFailed(string reason)
    {
        if (Status == VideoStatus.Ready)
            return Result.Failure(
                new Error("Video.InvalidTransition",
                    "Нельзя пометить готовое видео как ошибочное"));

        Status = VideoStatus.Failed;
        ErrorMessage = reason;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(
            new VideoProcessingFailedEvent(Id, LessonId, reason));

        return Result.Success();
    }

    public void IncrementRetry()
    {
        RetryCount++;
        UpdatedAt = DateTime.UtcNow;
    }

    // =========================
    // PLAYBACK
    // =========================

    public Result EnsureReadyForPlayback()
    {
        if (Status != VideoStatus.Ready)
        {
            return Status switch
            {
                VideoStatus.Uploading => Result.Failure(
                    new Error("Video.NotUploaded",
                        "Видео еще не загружено полностью")),

                VideoStatus.Uploaded => Result.Failure(
                    new Error("Video.NotProcessed",
                        "Видео ожидает обработки")),

                VideoStatus.Processing => Result.Failure(
                    new Error("Video.Processing",
                        "Видео находится в обработке. Попробуйте позже.")),

                VideoStatus.Failed => Result.Failure(
                    new Error("Video.Failed",
                        $"Ошибка обработки: {ErrorMessage}")),

                _ => Result.Failure(VideoErrors.NotFound)
            };
        }

        return Result.Success();
    }

    public Result<string> GetHlsPlaylistKey()
    {
        if (Status != VideoStatus.Ready || string.IsNullOrEmpty(StorageKey))
            return Result.Failure<string>(VideoErrors.MetadataNotAvailable);

        if (!StorageKey.EndsWith(".m3u8", StringComparison.OrdinalIgnoreCase))
        {
            return Result.Failure<string>(
                new Error("Video.InvalidKeyFormat",
                    "Ключ не указывает на HLS плейлист"));
        }

        return Result.Success(StorageKey);
    }

    public Result<VideoPlaybackMetadata> GetPlaybackMetadata()
    {
        if (Status != VideoStatus.Ready || DurationSeconds is null)
            return Result.Failure<VideoPlaybackMetadata>(
                VideoErrors.MetadataNotAvailable);

        return Result.Success(
            new VideoPlaybackMetadata(
                DurationSeconds.Value,
                Width ?? 0,
                Height ?? 0,
                Title.Value));
    }

    // =========================
    // EDITING
    // =========================

    public Result UpdateTitle(string newTitle)
    {
        var titleResult = VideoTitle.Create(newTitle);

        if (titleResult.IsFailure)
            return Result.Failure(titleResult.Error);

        Title = titleResult.Value;
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }
}

/// <summary>
/// DTO метаданных для плеера
/// </summary>
public record VideoPlaybackMetadata(
    int DurationSeconds,
    int Width,
    int Height,
    string Title);

