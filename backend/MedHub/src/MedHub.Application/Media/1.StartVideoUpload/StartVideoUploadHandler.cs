using MedHub.Application.Abstractions.Authentication;
using MedHub.Application.Abstractions.Messaging;
using MedHub.Application.Abstractions.Storage;
using MedHub.Application.Media.Contracts;
using MedHub.Domain.Abstractions;
using MedHub.Domain.Courses;
using MedHub.Domain.Lessons;
using MedHub.Domain.Media;

namespace MedHub.Application.Media.StartVideoUpload;

internal sealed class StartVideoUploadCommandHandler
    : ICommandHandler<StartVideoUploadCommand, StartVideoUploadResult>
{
    private const int DefaultChunkSize = 10 * 1024 * 1024; // 10 MB

    private readonly IVideoRepository _videoRepository;
    private readonly ILessonRepository _lessonRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly IVideoStorageProvider _storage;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserContext _userContext;

    public StartVideoUploadCommandHandler(
        IVideoRepository videoRepository,
        ILessonRepository lessonRepository,
        ICourseRepository courseRepository,
        IVideoStorageProvider storage,
        IUnitOfWork unitOfWork,
        IUserContext userContext)
    {
        _videoRepository = videoRepository;
        _lessonRepository = lessonRepository;
        _courseRepository = courseRepository;
        _storage = storage;
        _unitOfWork = unitOfWork;
        _userContext = userContext;
    }

    public async Task<Result<StartVideoUploadResult>> Handle(
        StartVideoUploadCommand command,
        CancellationToken ct)
    {
        var lesson = await _lessonRepository.GetByIdAsync(command.LessonId, ct);
        if (lesson is null)
        {
            return Result.Failure<StartVideoUploadResult>(LessonErrors.NotFound);
        }

        var course = await _courseRepository.GetByIdAsync(lesson.CourseId, ct);
        if (course is null)
        {
            return Result.Failure<StartVideoUploadResult>(CourseErrors.NotFound);
        }

        if (!_userContext.IsInRole("Admin") && course.CreatorId != _userContext.UserId)
        {
            return Result.Failure<StartVideoUploadResult>(
                new Error(
                    "Video.Forbidden",
                    "Только автор курса может загружать видео к уроку"));
        }

        var title = Path.GetFileNameWithoutExtension(command.FileName);

        var videoResult = VideoMaterial.Create(
            command.LessonId,
            title,
            command.FileName);

        if (videoResult.IsFailure)
        {
            return Result.Failure<StartVideoUploadResult>(videoResult.Error);
        }

        var video = videoResult.Value;

        var objectKey = $"videos/{video.Id:N}/raw/{command.FileName}";

        var init = await _storage.StartMultipartUploadAsync(
            objectKey,
            command.ContentType,
            command.SizeBytes,
            ct);

        video.StartUpload(objectKey, init.UploadId, command.SizeBytes);

        await _videoRepository.AddAsync(video, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        var partsCount = (int)Math.Ceiling((double)command.SizeBytes / DefaultChunkSize);

        IReadOnlyList<ChunkUploadUrlDto> urlsDto = await _storage.GetUploadUrlsAsync(
            objectKey,
            init.UploadId,
            partsCount,
            ct);

        List<ChunkUploadUrl> urls = urlsDto
            .Select(x => new ChunkUploadUrl(
                x.PartNumber,
                x.UploadUrl))
            .ToList();

        return Result.Success(new StartVideoUploadResult(
            video.Id,
            init.UploadId,
            DefaultChunkSize,
            urls));
    }
}
