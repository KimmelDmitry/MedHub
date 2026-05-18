using FluentValidation;

namespace MedHub.Application.Questions.AddAnswerOption;

internal sealed class AddAnswerOptionCommandValidator
    : AbstractValidator<AddAnswerOptionCommand>
{
    public AddAnswerOptionCommandValidator()
    {
        RuleFor(x => x.QuestionId)
            .NotEmpty();

        RuleFor(x => x.Text)
            .NotEmpty()
            .MaximumLength(1000);
    }
}