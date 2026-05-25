using MedHub.Application.Abstractions.Authentication;
using MedHub.Application.Abstractions.Messaging;
using MedHub.Domain.Abstractions;
using MedHub.Domain.Checkpoints;
using MedHub.Domain.Courses;
using MedHub.Domain.Lessons;
using MedHub.Domain.Media;

namespace MedHub.Application.Checkpoints.GetVideoCheckpoints;

internal sealed class GetVideoCheckpointsQueryHandler
    : IQueryHandler<GetVideoCheckpointsQuery, IReadOnlyList<VideoCheckpointResponse>>
{
    private readonly IVideoRepository _videoRepository;
    private readonly ILessonRepository _lessonRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly ICheckpointRepository _checkpointRepository;
    private readonly IUserContext _userContext;

    public GetVideoCheckpointsQueryHandler(
        IVideoRepository videoRepository,
        ILessonRepository lessonRepository,
        ICourseRepository courseRepository,
        ICheckpointRepository checkpointRepository,
        IUserContext userContext)
    {
        _videoRepository = videoRepository;
        _lessonRepository = lessonRepository;
        _courseRepository = courseRepository;
        _checkpointRepository = checkpointRepository;
        _userContext = userContext;
    }

    public async Task<Result<IReadOnlyList<VideoCheckpointResponse>>> Handle(
        GetVideoCheckpointsQuery query,
        CancellationToken cancellationToken)
    {
        VideoMaterial? video = await _videoRepository.GetByIdAsync(query.VideoId, cancellationToken);

        if (video is null)
        {
            return Result.Failure<IReadOnlyList<VideoCheckpointResponse>>(VideoErrors.NotFound);
        }

        var accessResult = await CheckpointAccess.EnsureCanManageVideoAsync(
            video,
            _lessonRepository,
            _courseRepository,
            _userContext,
            cancellationToken);

        if (accessResult.IsFailure)
        {
            return Result.Failure<IReadOnlyList<VideoCheckpointResponse>>(accessResult.Error);
        }

        var checkpoints = await _checkpointRepository.GetByVideoIdAsync(
            query.VideoId,
            cancellationToken);

        var response = checkpoints
            .OrderBy(x => x.Timestamp.Value)
            .ThenBy(x => x.OrderNumber)
            .Select(x => new VideoCheckpointResponse(
                x.Id,
                x.Timestamp.Value,
                x.OrderNumber,
                x.Title,
                x.IsRequired,
                x.IsGraded,
                x.Status.ToString(),
                x.Questions.Count))
            .ToList();

        return Result.Success<IReadOnlyList<VideoCheckpointResponse>>(response);
    }
}
