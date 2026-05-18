using FluentValidation;

namespace MedHub.Application.Courses.PublishCourse;

internal sealed class PublishCourseCommandValidator
    : AbstractValidator<PublishCourseCommand>
{
    public PublishCourseCommandValidator()
    {
        RuleFor(x => x.CourseId)
            .NotEmpty();
    }
}