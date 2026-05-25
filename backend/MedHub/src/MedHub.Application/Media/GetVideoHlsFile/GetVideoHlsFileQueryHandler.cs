using System.Text;
using MedHub.Application.Abstractions.Authentication;
using MedHub.Application.Abstractions.Messaging;
using MedHub.Application.Abstractions.Storage;
using MedHub.Domain.Abstractions;
using MedHub.Domain.Courses;
using MedHub.Domain.Enrollments;
using MedHub.Domain.Lessons;
using MedHub.Domain.Media;

namespace MedHub.Application.Media.GetVideoHlsFile;

internal sealed class GetVideoHlsFileQueryHandler
    : IQueryHandler<GetVideoHlsFileQuery, VideoHlsFileResponse>
{
    private const string MasterPlaylistFileName = "master.m3u8";
    private const string MpegUrlContentType = "application/vnd.apple.mpegurl";
    private const string TransportStreamContentType = "video/mp2t";

    private readonly IVideoRepository _videoRepository;
    private readonly ILessonRepository _lessonRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly IEnrollmentRepository _enrollmentRepository;
    private readonly IVideoStorageProvider _storageProvider;
    private readonly IUserContext _userContext;

    public GetVideoHlsFileQueryHandler(
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

    public async Task<Result<VideoHlsFileResponse>> Handle(
        GetVideoHlsFileQuery query,
        CancellationToken cancellationToken)
    {
        string fileName = query.FileName.Trim();

        if (!IsAllowedHlsFileName(fileName))
        {
            return Result.Failure<VideoHlsFileResponse>(
                new Error(
                    "Video.InvalidHlsFile",
                    "Invalid HLS file name"));
        }

        VideoMaterial? video = await _videoRepository.GetByIdAsync(query.VideoId, cancellationToken);

        if (video is null)
        {
            return Result.Failure<VideoHlsFileResponse>(VideoErrors.NotFound);
        }

        Result readyResult = video.EnsureReadyForPlayback();

        if (readyResult.IsFailure)
        {
            return Result.Failure<VideoHlsFileResponse>(readyResult.Error);
        }

        Result accessResult = await EnsureAccessAsync(video, cancellationToken);

        if (accessResult.IsFailure)
        {
            return Result.Failure<VideoHlsFileResponse>(accessResult.Error);
        }

        Result<string> masterKeyResult = video.GetHlsPlaylistKey();

        if (masterKeyResult.IsFailure)
        {
            return Result.Failure<VideoHlsFileResponse>(masterKeyResult.Error);
        }

        string masterKey = masterKeyResult.Value;
        string expectedPrefix = $"videos/{video.Id:N}/hls/";

        if (!masterKey.Equals($"{expectedPrefix}{MasterPlaylistFileName}", StringComparison.Ordinal))
        {
            return Result.Failure<VideoHlsFileResponse>(
                new Error(
                    "Video.InvalidHlsKey",
                    "Invalid HLS storage key"));
        }

        string objectKey = fileName == MasterPlaylistFileName
            ? masterKey
            : $"{expectedPrefix}{fileName}";

        if (!objectKey.StartsWith(expectedPrefix, StringComparison.Ordinal))
        {
            return Result.Failure<VideoHlsFileResponse>(
                new Error(
                    "Video.InvalidHlsKey",
                    "Invalid HLS storage key"));
        }

        StorageObjectStream storageObject =
            await _storageProvider.OpenReadAsync(objectKey, cancellationToken);

        if (fileName == MasterPlaylistFileName)
        {
            return await RewritePlaylistAsync(
                query.VideoId,
                storageObject.Content,
                cancellationToken);
        }

        return Result.Success(new VideoHlsFileResponse(
            storageObject.Content,
            TransportStreamContentType,
            storageObject.ContentLength));
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

    private static async Task<Result<VideoHlsFileResponse>> RewritePlaylistAsync(
        Guid videoId,
        Stream playlistStream,
        CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(
            playlistStream,
            Encoding.UTF8,
            detectEncodingFromByteOrderMarks: true,
            leaveOpen: false);

        string playlist = await reader.ReadToEndAsync(cancellationToken);
        string[] lines = playlist.Replace("\r\n", "\n").Split('\n');

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i].Trim();

            if (line.Length == 0 || line.StartsWith('#'))
            {
                continue;
            }

            if (line.StartsWith('/') ||
                line.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                line.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (IsSegmentFileName(line))
            {
                lines[i] = $"/api/v1/media/videos/{videoId}/hls/{line}";
            }
        }

        byte[] bytes = Encoding.UTF8.GetBytes(string.Join('\n', lines));

        return Result.Success(new VideoHlsFileResponse(
            new MemoryStream(bytes),
            MpegUrlContentType,
            bytes.Length));
    }

    private static bool IsAllowedHlsFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName) ||
            fileName.Contains('/') ||
            fileName.Contains('\\') ||
            fileName.Contains("..", StringComparison.Ordinal))
        {
            return false;
        }

        return fileName == MasterPlaylistFileName || IsSegmentFileName(fileName);
    }

    private static bool IsSegmentFileName(string fileName)
    {
        const string prefix = "segment_";
        const string suffix = ".ts";

        if (!fileName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) ||
            !fileName.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        string number = fileName[prefix.Length..^suffix.Length];

        return number.Length > 0 && number.All(char.IsDigit);
    }
}
