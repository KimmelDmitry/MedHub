namespace MedHub.Application.Media.GetVideoPlayback;

public sealed record VideoPlaybackResponse(
    Guid VideoId,
    string PlaybackUrl,
    int DurationSeconds,
    int Width,
    int Height,
    string Title
);