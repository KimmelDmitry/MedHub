using MedHub.Application.Abstractions.Messaging;
using MedHub.Domain.Abstractions;
using MedHub.Domain.Lessons;

namespace MedHub.Application.Lessons.UpdateLessonContent;

internal sealed class UpdateLessonContentCommandHandler
    : ICommandHandler<UpdateLessonContentCommand>
{
    private readonly ILessonRepository _lessonRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateLessonContentCommandHandler(
        ILessonRepository lessonRepository,
        IUnitOfWork unitOfWork)
    {
        _lessonRepository = lessonRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        UpdateLessonContentCommand command,
        CancellationToken cancellationToken)
    {
        Lesson? lesson = await _lessonRepository.GetByIdAsync(
            command.LessonId,
            cancellationToken);

        if (lesson is null)
        {
            return Result.Failure(LessonErrors.NotFound);
        }

        Result result = lesson.UpdateContent(
            command.ContentUrl,
            command.ContentType);

        if (result.IsFailure)
        {
            return result;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}