using MedHub.Application.Media.Options;
using MedHub.Application.Media.Services;
using Microsoft.Extensions.Options;

namespace MedHub.Infrastructure.BackgroundJobs.MediaProcessing;

internal sealed class FfmpegVideoTranscoder : IVideoTranscoder
{
    private readonly VideoProcessingOptions _options;

    public FfmpegVideoTranscoder(IOptions<VideoProcessingOptions> options)
    {
        _options = options.Value;
    }

    public async Task TranscodeToHlsAsync(
        string inputPath,
        string outputDirectory,
        CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(outputDirectory);

        var playlistPath = Path.Combine(outputDirectory, "master.m3u8");
        var segmentPath = Path.Combine(outputDirectory, "segment_%03d.ts");

        var args =
            $"-y -i \"{inputPath}\" " +
            "-map 0:v:0 -map 0:a:0? " +
            "-c:v libx264 -preset veryfast -crf 23 " +
            "-c:a aac -b:a 128k " +
            "-f hls " +
            $"-hls_time {_options.HlsSegmentDurationSeconds} " +
            "-hls_playlist_type vod " +
            "-hls_flags independent_segments " +
            $"-hls_segment_filename \"{segmentPath}\" " +
            $"\"{playlistPath}\"";

        await ProcessHelper.RunProcessAsync(
            _options.FfmpegPath,
            args,
            _options.TranscodeTimeout,
            cancellationToken);
    }
}