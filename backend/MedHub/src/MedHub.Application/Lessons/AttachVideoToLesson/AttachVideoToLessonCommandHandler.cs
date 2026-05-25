using MedHub.Application.Abstractions.Authentication;
using MedHub.Application.Abstractions.Messaging;
using MedHub.Domain.Abstractions;
using MedHub.Domain.Courses;
using MedHub.Domain.Lessons;
using MedHub.Domain.Media;

namespace MedHub.Application.Lessons.AttachVideoToLesson;

internal sealed class AttachVideoToLessonCommandHandler
    : ICommandHandler<AttachVideoToLessonCommand>
{
    private readonly ILessonRepository _lessonRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly IVideoRepository _videoRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserContext _userContext;

    public AttachVideoToLessonCommandHandler(
        ILessonRepository lessonRepository,
        ICourseRepository courseRepository,
        IVideoRepository videoRepository,
        IUnitOfWork unitOfWork,
        IUserContext userContext)
    {
        _lessonRepository = lessonRepository;
        _courseRepository = courseRepository;
        _videoRepository = videoRepository;
        _unitOfWork = unitOfWork;
        _userContext = userContext;
    }

    public async Task<Result> Handle(
        AttachVideoToLessonCommand command,
        CancellationToken ct)
    {
        var lesson = await _lessonRepository.GetByIdAsync(command.LessonId, ct);

        if (lesson is null)
        {
            return Result.Failure(LessonErrors.NotFound);
        }

        var course = await _courseRepository.GetByIdAsync(lesson.CourseId, ct);

        if (course is null)
        {
            return Result.Failure(CourseErrors.NotFound);
        }

        if (!_userContext.IsInRole("Admin") && course.CreatorId != _userContext.UserId)
        {
            return Result.Failure(
                new Error(
                    "Lesson.Forbidden",
                    "Только автор курса может прикреплять видео к уроку"));
        }

        VideoMaterial? video = await _videoRepository.GetByIdAsync(command.VideoId, ct);

        if (video is null)
        {
            return Result.Failure(VideoErrors.NotFound);
        }

        if (video.LessonId != lesson.Id)
        {
            return Result.Failure(
                new Error(
                    "Lesson.VideoMismatch",
                    "Видео было загружено для другого урока"));
        }

        if (video.Status != VideoStatus.Ready)
        {
            return Result.Failure(
                new Error(
                    "Lesson.VideoNotReady",
                    "Видео еще не обработано"));
        }

        var result = lesson.AttachVideo(video.Id);

        if (result.IsFailure)
        {
            return result;
        }

        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}
