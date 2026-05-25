using MedHub.Application.Abstractions.Messaging;
using MedHub.Application.Abstractions.Authentication;
using MedHub.Application.Abstractions.Storage;
using MedHub.Domain.Abstractions;
using MedHub.Domain.Courses;
using MedHub.Domain.Enrollments;
using MedHub.Domain.Lessons;
using MedHub.Domain.Media;

namespace MedHub.Application.Media.GetVideoPlayback;

internal sealed class GetVideoPlaybackQueryHandler
    : IQueryHandler<GetVideoPlaybackQuery, VideoPlaybackResponse>
{
    private readonly IVideoRepository _videoRepository;
    private readonly ILessonRepository _lessonRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly IEnrollmentRepository _enrollmentRepository;
    private readonly IVideoStorageProvider _storageProvider;
    private readonly IUserContext _userContext;

    public GetVideoPlaybackQueryHandler(
        IVideoRepository videoRepository,
        ILessonRepository lessonRepository,
        ICourseRepository courseRepository,
        IEnrollmentRepository enrollmentRepository,
        IVideoStorageProvider storageProvider,
        IUserContext userContext)
    {
        _videoRepository = videoRepository;
        _lessonRepository = lessonRepository;
        _courseRepository = courseRepository;
        _enrollmentRepository = enrollmentRepository;
        _storageProvider = storageProvider;
        _userContext = userContext;
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

        var accessResult = await EnsureAccessAsync(video, cancellationToken);

        if (accessResult.IsFailure)
        {
            return Result.Failure<VideoPlaybackResponse>(
                accessResult.Error);
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

    private async Task<Result> EnsureAccessAsync(
        VideoMaterial video,
        CancellationToken cancellationToken)
    {
        Lesson? lesson = await _lessonRepository.GetByIdAsync(video.LessonId, cancellationToken);

        if (lesson is null)
        {
            return Result.Failure(LessonErrors.NotFound);
        }

        Course? course = await _courseRepository.GetByIdAsync(lesson.CourseId, cancellationToken);

        if (course is null)
        {
            return Result.Failure(CourseErrors.NotFound);
        }

        if (_userContext.IsInRole("Admin") || course.CreatorId == _userContext.UserId)
        {
            return Result.Success();
        }

        if (_userContext.IsInRole("Student") &&
            course.Status == CourseStatus.Published &&
            lesson.Status == LessonStatus.Published &&
            lesson.VideoId == video.Id &&
            video.Status == VideoStatus.Ready)
        {
            bool hasActiveEnrollment = await _enrollmentRepository.IsActiveAsync(
                _userContext.UserId,
                course.Id,
                cancellationToken);

            return hasActiveEnrollment
                ? Result.Success()
                : Result.Failure(EnrollmentErrors.Required);
        }

        return Result.Failure(
            new Error(
                "Video.Forbidden",
                "Only the course author or an eligible student can access this video"));
    }
}
