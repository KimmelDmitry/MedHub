namespace MedHub.Application.Media.GetVideoStatus;

public sealed record VideoStatusResponse(
    Guid VideoId,
    string Status,
    string? ErrorMessage,
    int? DurationSeconds,
    int? Width,
    int? Height
);