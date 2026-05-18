namespace MedHub.Application.Media.Contracts;

public sealed record VideoStatusDto(
    Guid VideoId,
    string Status,
    string? PlaybackUrl,
    string? ErrorMessage
);