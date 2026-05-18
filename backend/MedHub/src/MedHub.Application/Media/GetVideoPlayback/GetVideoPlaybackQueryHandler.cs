using MedHub.Application.Abstractions.Messaging;
using MedHub.Application.Abstractions.Storage;
using MedHub.Domain.Abstractions;
using MedHub.Domain.Media;

namespace MedHub.Application.Media.GetVideoPlayback;

internal sealed class GetVideoPlaybackQueryHandler
    : IQueryHandler<GetVideoPlaybackQuery, VideoPlaybackResponse>
{
    private readonly IVideoRepository _videoRepository;
    private readonly IVideoStorageProvider _storageProvider;

    public GetVideoPlaybackQueryHandler(
        IVideoRepository videoRepository,
        IVideoStorageProvider storageProvider)
    {
        _videoRepository = videoRepository;
        _storageProvider = storageProvider;
    }

    public async Task<Result<VideoPlaybackResponse>> Handle(
        GetVideoPlaybackQuery request,
        CancellationToken cancellationToken)
    {
        var video = await _videoRepository.GetByIdAsync(
            request.VideoId,
            cancellationToken);

        if (video is null)
        {
            return Result.Failure<VideoPlaybackResponse>(
                VideoErrors.NotFound);
        }

        var readyResult = video.EnsureReadyForPlayback();

        if (readyResult.IsFailure)
        {
            return Result.Failure<VideoPlaybackResponse>(
                readyResult.Error);
        }

        var keyResult = video.GetHlsPlaylistKey();

        if (keyResult.IsFailure)
        {
            return Result.Failure<VideoPlaybackResponse>(
                keyResult.Error);
        }

        var metadataResult = video.GetPlaybackMetadata();

        if (metadataResult.IsFailure)
        {
            return Result.Failure<VideoPlaybackResponse>(
                metadataResult.Error);
        }

        var playbackUrl = await _storageProvider.GetPlaybackUrlAsync(
            keyResult.Value,
            cancellationToken);

        var metadata = metadataResult.Value;

        return Result.Success(
            new VideoPlaybackResponse(
                video.Id,
                playbackUrl,
                metadata.DurationSeconds,
                metadata.Width,
                metadata.Height,
                metadata.Title));
    }
}