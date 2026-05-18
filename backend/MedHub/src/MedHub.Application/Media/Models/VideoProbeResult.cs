namespace MedHub.Application.Media.Models;

public sealed record VideoProbeResult(
    TimeSpan? Duration,
    int? Width,
    int? Height,
    string? VideoCodec,
    string? AudioCodec,
    long? BitRate
);