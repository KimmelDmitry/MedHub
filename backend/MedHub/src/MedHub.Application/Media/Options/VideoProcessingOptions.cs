namespace MedHub.Application.Media.Options;

public sealed class VideoProcessingOptions
{
    public const string SectionName = "VideoProcessing";

    public string TempRootPath { get; init; } = Path.Combine(Path.GetTempPath(), "medhub-videos");
    public TimeSpan PollInterval { get; init; } = TimeSpan.FromSeconds(5);

    public string FfprobePath { get; init; } = "ffprobe";
    public string FfmpegPath { get; init; } = "ffmpeg";

    public TimeSpan ProbeTimeout { get; init; } = TimeSpan.FromMinutes(5);
    public TimeSpan TranscodeTimeout { get; init; } = TimeSpan.FromMinutes(30);

    public int HlsSegmentDurationSeconds { get; init; } = 6;
}