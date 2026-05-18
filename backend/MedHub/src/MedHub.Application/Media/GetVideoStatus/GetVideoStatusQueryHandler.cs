using MedHub.Application.Abstractions.Messaging;
using MedHub.Domain.Abstractions;
using MedHub.Domain.Media;

namespace MedHub.Application.Media.GetVideoStatus;

internal sealed class GetVideoStatusQueryHandler
    : IQueryHandler<GetVideoStatusQuery, VideoStatusResponse>
{
    private readonly IVideoRepository _videoRepository;

    public GetVideoStatusQueryHandler(IVideoRepository videoRepository)
    {
        _videoRepository = videoRepository;
    }

    public async Task<Result<VideoStatusResponse>> Handle(
        GetVideoStatusQuery request,
        CancellationToken cancellationToken)
    {
        var video = await _videoRepository.GetByIdAsync(
            request.VideoId,
            cancellationToken);

        if (video is null)
        {
            return Result.Failure<VideoStatusResponse>(
                VideoErrors.NotFound);
        }

        var response = new VideoStatusResponse(
            video.Id,
            video.Status.ToString(),
            video.ErrorMessage,
            video.DurationSeconds,
            video.Width,
            video.Height);

        return Result.Success(response);
    }
}