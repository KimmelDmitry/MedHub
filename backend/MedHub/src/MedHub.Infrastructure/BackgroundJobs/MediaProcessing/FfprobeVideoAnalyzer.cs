using System.Globalization;
using System.Text.Json;
using MedHub.Application.Media.Models;
using MedHub.Application.Media.Options;
using MedHub.Application.Media.Services;
using Microsoft.Extensions.Options;

namespace MedHub.Infrastructure.BackgroundJobs.MediaProcessing;

internal sealed class FfprobeVideoAnalyzer : IVideoProbeAnalyzer
{
    private readonly VideoProcessingOptions _options;

    public FfprobeVideoAnalyzer(IOptions<VideoProcessingOptions> options)
    {
        _options = options.Value;
    }

    public async Task<VideoProbeResult> AnalyzeAsync(
        string filePath,
        CancellationToken cancellationToken = default)
    {
        var args = $"-v error -print_format json -show_format -show_streams \"{filePath}\"";

        var json = await ProcessHelper.RunProcessAsync(
            _options.FfprobePath,
            args,
            _options.ProbeTimeout,
            cancellationToken);

        using var doc = JsonDocument.Parse(json);

        TimeSpan? duration = null;
        long? bitRate = null;

        if (doc.RootElement.TryGetProperty("format", out var format))
        {
            if (format.TryGetProperty("duration", out var durationEl) &&
                double.TryParse(durationEl.GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var seconds))
            {
                duration = TimeSpan.FromSeconds(seconds);
            }

            if (format.TryGetProperty("bit_rate", out var bitRateEl) &&
                long.TryParse(bitRateEl.GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var br))
            {
                bitRate = br;
            }
        }

        int? width = null;
        int? height = null;
        string? videoCodec = null;
        string? audioCodec = null;

        if (doc.RootElement.TryGetProperty("streams", out var streams) &&
            streams.ValueKind == JsonValueKind.Array)
        {
            foreach (var stream in streams.EnumerateArray())
            {
                var codecType = stream.TryGetProperty("codec_type", out var codecTypeEl)
                    ? codecTypeEl.GetString()
                    : null;

                if (codecType == "video")
                {
                    videoCodec = stream.TryGetProperty("codec_name", out var codecNameEl)
                        ? codecNameEl.GetString()
                        : null;

                    if (stream.TryGetProperty("width", out var widthEl) && widthEl.TryGetInt32(out var w))
                        width = w;

                    if (stream.TryGetProperty("height", out var heightEl) && heightEl.TryGetInt32(out var h))
                        height = h;
                }
                else if (codecType == "audio")
                {
                    audioCodec = stream.TryGetProperty("codec_name", out var codecNameEl)
                        ? codecNameEl.GetString()
                        : null;
                }
            }
        }

        return new VideoProbeResult(duration, width, height, videoCodec, audioCodec, bitRate);
    }
}

