using MedHub.Application.Abstractions.Messaging;
using MedHub.Domain.Abstractions;
using MedHub.Domain.Lessons;
using MedHub.Domain.Media;

namespace MedHub.Application.Lessons.AttachVideoToLesson;

internal sealed class AttachVideoToLessonCommandHandler
    : ICommandHandler<AttachVideoToLessonCommand>
{
    private readonly ILessonRepository _lessonRepository;
    private readonly IVideoRepository _videoRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AttachVideoToLessonCommandHandler(
        ILessonRepository lessonRepository,
        IVideoRepository videoRepository,
        IUnitOfWork unitOfWork)
    {
        _lessonRepository = lessonRepository;
        _videoRepository = videoRepository;
        _unitOfWork = unitOfWork;
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

        VideoMaterial? video = await _videoRepository.GetByIdAsync(command.VideoId, ct);

        if (video is null)
        {
            return Result.Failure(VideoErrors.NotFound);
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