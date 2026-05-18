using FluentValidation;

namespace MedHub.Application.Courses.UpdateCourseDescription;

internal sealed class UpdateCourseDescriptionCommandValidator
    : AbstractValidator<UpdateCourseDescriptionCommand>
{
    public UpdateCourseDescriptionCommandValidator()
    {
        RuleFor(x => x.CourseId)
            .NotEmpty();

        RuleFor(x => x.Description)
            .MaximumLength(2000);
    }
}