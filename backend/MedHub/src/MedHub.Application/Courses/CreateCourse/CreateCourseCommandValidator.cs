using FluentValidation;

namespace MedHub.Application.Courses.CreateCourse;

internal sealed class CreateCourseCommandValidator : AbstractValidator<CreateCourseCommand>
{
    public CreateCourseCommandValidator()
    {
        RuleFor(c => c.Title)
            .NotEmpty().WithMessage("Название курса обязательно")
            .MinimumLength(3).WithMessage("Название должно быть не менее 3 символов")
            .MaximumLength(200).WithMessage("Название не может превышать 200 символов");

        RuleFor(c => c.Description)
            .MaximumLength(2000).When(c => !string.IsNullOrEmpty(c.Description))
            .WithMessage("Описание не может превышать 2000 символов");
    }
}