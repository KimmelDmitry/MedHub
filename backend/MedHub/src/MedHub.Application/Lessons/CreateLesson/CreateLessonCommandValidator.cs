using FluentValidation;
using MedHub.Domain.Lessons;

namespace MedHub.Application.Lessons.CreateLesson;

internal sealed class CreateLessonCommandValidator : AbstractValidator<CreateLessonCommand>
{
    public CreateLessonCommandValidator()
    {
        RuleFor(c => c.CourseId).NotEmpty().WithMessage("ID курса обязателен");

        RuleFor(c => c.Title)
            .NotEmpty().WithMessage("Название урока обязательно")
            .MinimumLength(3).WithMessage("Название должно быть не менее 3 символов")
            .MaximumLength(150).WithMessage("Название не может превышать 150 символов");

        RuleFor(c => c.Order).GreaterThan(0).WithMessage("Порядок урока должен быть больше 0");

        RuleFor(c => c.ContentType).IsInEnum().WithMessage("Неверный тип контента");

        // Если контент текстовый, URL обязателен (или контент сам по себе, но пока используем URL как заглушку)
        RuleFor(c => c.ContentUrl)
            .NotEmpty().When(c => c.ContentType == LessonContentType.Text)
            .WithMessage("Для текстового урока необходим контент (URL или текст)");
    }
}