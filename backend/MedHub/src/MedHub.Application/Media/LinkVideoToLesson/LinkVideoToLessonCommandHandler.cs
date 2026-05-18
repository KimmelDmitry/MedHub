using MedHub.Application.Abstractions.Messaging;
using MedHub.Domain.Abstractions;
using MedHub.Domain.Lessons;
using MedHub.Domain.Media;

namespace MedHub.Application.Media.LinkVideoToLesson;

internal sealed class LinkVideoToLessonCommandHandler
    : ICommandHandler<LinkVideoToLessonCommand>
{
    private readonly ILessonRepository _lessonRepository;
    private readonly IVideoRepository _videoRepository;
    private readonly IUnitOfWork _unitOfWork;

    public LinkVideoToLessonCommandHandler(
        ILessonRepository lessonRepository,
        IVideoRepository videoRepository,
        IUnitOfWork unitOfWork)
    {
        _lessonRepository = lessonRepository;
        _videoRepository = videoRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        LinkVideoToLessonCommand command,
        CancellationToken cancellationToken)
    {
        Lesson? lesson = await _lessonRepository.GetByIdAsync(
            command.LessonId,
            cancellationToken);

        if (lesson is null)
        {
            return Result.Failure(
                new Error(
                    "Lesson.NotFound",
                    "Урок не найден"));
        }

        var video = await _videoRepository.GetByIdAsync(
            command.VideoId,
            cancellationToken);

        if (video is null)
        {
            return Result.Failure(VideoErrors.NotFound);
        }

        var readyResult = video.EnsureReadyForPlayback();

        if (readyResult.IsFailure)
        {
            return Result.Failure(readyResult.Error);
        }

        lesson.AttachVideo(command.VideoId);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}