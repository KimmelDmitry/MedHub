using MedHub.Application.Abstractions.Media;
using MedHub.Application.Abstractions.Storage;
using MedHub.Application.Media.Options;
using MedHub.Application.Media.Services;
using MedHub.Domain.Abstractions;
using MedHub.Domain.Media;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MedHub.Infrastructure.BackgroundJobs.MediaProcessing;

internal sealed class VideoProcessingService : IVideoProcessingService
{
    private readonly IVideoRepository _videoRepository;
    private readonly IVideoStorageProvider _storage;
    private readonly IVideoProbeAnalyzer _probeAnalyzer;
    private readonly IVideoTranscoder _transcoder;
    private readonly IUnitOfWork _unitOfWork;
    private readonly VideoProcessingOptions _options;
    private readonly ILogger<VideoProcessingService> _logger;

    public VideoProcessingService(
        IVideoRepository videoRepository,
        IVideoStorageProvider storage,
        IVideoProbeAnalyzer probeAnalyzer,
        IVideoTranscoder transcoder,
        IUnitOfWork unitOfWork,
        IOptions<VideoProcessingOptions> options,
        ILogger<VideoProcessingService> logger)
    {
        _videoRepository = videoRepository;
        _storage = storage;
        _probeAnalyzer = probeAnalyzer;
        _transcoder = transcoder;
        _unitOfWork = unitOfWork;
        _options = options.Value;
        _logger = logger;
    }

    public async Task ProcessNextUploadedAsync(CancellationToken cancellationToken = default)
    {
        var next = await _videoRepository.GetNextUploadedAsync(cancellationToken);
        if (next is null)
        {
            return;
        }

        await ProcessAsync(next.Id, cancellationToken);
    }

    public async Task ProcessAsync(Guid videoId, CancellationToken cancellationToken = default)
    {
        var video = await _videoRepository.GetByIdAsync(videoId, cancellationToken);
        if (video is null)
        {
            return;
        }

        if (video.Status != VideoStatus.Uploaded)
        {
            return;
        }

        var claimed = await _videoRepository.TryClaimForProcessingAsync(video.Id, cancellationToken);
        if (!claimed)
        {
            return;
        }

        var startResult = video.StartProcessing();
        if (startResult.IsFailure)
        {
            return;
        }

        var workingDir = Path.Combine(_options.TempRootPath, video.Id.ToString("N"));

        var sourceKey = video.StorageKey;
        if (string.IsNullOrWhiteSpace(sourceKey))
        {
            video.MarkAsFailed("StorageKey is empty");
            await _videoRepository.UpdateAsync(video, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return;
        }

        var sourcePath = Path.Combine(workingDir, $"source{Path.GetExtension(sourceKey)}");
        var outputDir = Path.Combine(workingDir, "hls");

        try
        {
            Directory.CreateDirectory(workingDir);

            await _storage.DownloadFileAsync(sourceKey, sourcePath, cancellationToken);

            var probe = await _probeAnalyzer.AnalyzeAsync(sourcePath, cancellationToken);
            if (probe.Duration is null || probe.Duration.Value <= TimeSpan.Zero)
            {
                throw new InvalidOperationException("Video duration was not detected.");
            }

            await _transcoder.TranscodeToHlsAsync(sourcePath, outputDir, cancellationToken);

            var hlsPrefix = $"videos/{video.Id:N}/hls";
            await UploadDirectoryAsync(outputDir, hlsPrefix, cancellationToken);

            var masterKey = $"{hlsPrefix}/master.m3u8";

            Result completeResult = video.CompleteProcessing(
                (int)Math.Ceiling(probe.Duration.Value.TotalSeconds),
                probe.Width ?? 0,
                probe.Height ?? 0,
                masterKey);

            if (completeResult.IsFailure)
            {
                throw new InvalidOperationException(completeResult.Error.Name);
            }

            await _videoRepository.UpdateAsync(video, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Video processing failed for VideoId={VideoId}", videoId);

            video.IncrementRetry();
            video.MarkAsFailed(ex.Message);

            await _videoRepository.UpdateAsync(video, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        finally
        {
            try
            {
                if (Directory.Exists(workingDir))
                {
                    Directory.Delete(workingDir, true);
                }
            }
            catch
            {
                // ignore
            }
        }
    }

    private async Task UploadDirectoryAsync(
        string directoryPath,
        string prefix,
        CancellationToken cancellationToken)
    {
        foreach (var filePath in Directory.EnumerateFiles(directoryPath, "*", SearchOption.AllDirectories))
        {
            var relativePath = Path.GetRelativePath(directoryPath, filePath).Replace('\\', '/');
            var objectKey = $"{prefix}/{relativePath}";

            await using var stream = File.OpenRead(filePath);
            await _storage.UploadStreamAsync(
                objectKey,
                stream,
                GetContentType(filePath),
                cancellationToken);
        }
    }

    private static string GetContentType(string filePath)
    {
        return Path.GetExtension(filePath).ToLowerInvariant() switch
        {
            ".m3u8" => "application/vnd.apple.mpegurl",
            ".ts" => "video/mp2t",
            ".mp4" => "video/mp4",
            _ => "application/octet-stream"
        };
    }
}
