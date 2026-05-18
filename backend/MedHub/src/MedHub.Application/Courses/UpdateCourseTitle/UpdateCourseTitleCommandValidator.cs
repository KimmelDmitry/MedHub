using FluentValidation;

namespace MedHub.Application.Courses.UpdateCourseTitle;

internal sealed class UpdateCourseTitleCommandValidator
    : AbstractValidator<UpdateCourseTitleCommand>
{
    public UpdateCourseTitleCommandValidator()
    {
        RuleFor(x => x.CourseId)
            .NotEmpty();

        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(200);
    }
}