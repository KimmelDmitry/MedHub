using FluentValidation;

namespace MedHub.Application.Questions.UpdateQuestionSettings;

internal sealed class UpdateQuestionSettingsCommandValidator
    : AbstractValidator<UpdateQuestionSettingsCommand>
{
    public UpdateQuestionSettingsCommandValidator()
    {
        RuleFor(x => x.QuestionId)
            .NotEmpty();

        RuleFor(x => x.TimeLimitSeconds)
            .GreaterThan(0)
            .When(x => x.TimeLimitSeconds.HasValue);

        RuleFor(x => x.CorrectTextAnswer)
            .MaximumLength(1000);
    }
}