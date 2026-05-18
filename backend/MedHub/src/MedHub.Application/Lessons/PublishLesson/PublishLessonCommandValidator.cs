using FluentValidation;

namespace MedHub.Application.Lessons.PublishLesson;

internal sealed class PublishLessonCommandValidator
    : AbstractValidator<PublishLessonCommand>
{
    public PublishLessonCommandValidator()
    {
        RuleFor(x => x.LessonId)
            .NotEmpty();
    }
}