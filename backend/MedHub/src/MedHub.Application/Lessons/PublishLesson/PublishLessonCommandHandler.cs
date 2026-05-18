using MedHub.Application.Abstractions.Messaging;
using MedHub.Domain.Abstractions;
using MedHub.Domain.Lessons;

namespace MedHub.Application.Lessons.PublishLesson;

internal sealed class PublishLessonCommandHandler
    : ICommandHandler<PublishLessonCommand>
{
    private readonly ILessonRepository _lessonRepository;
    private readonly IUnitOfWork _unitOfWork;

    public PublishLessonCommandHandler(
        ILessonRepository lessonRepository,
        IUnitOfWork unitOfWork)
    {
        _lessonRepository = lessonRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        PublishLessonCommand command,
        CancellationToken ct)
    {
        var lesson = await _lessonRepository.GetByIdAsync(command.LessonId, ct);

        if (lesson is null)
        {
            return Result.Failure(LessonErrors.NotFound);
        }

        var result = lesson.Publish();

        if (result.IsFailure)
        {
            return result;
        }

        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}