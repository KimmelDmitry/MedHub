using FluentValidation;

namespace MedHub.Application.Questions.DeleteQuestion;

internal sealed class DeleteQuestionCommandValidator
    : AbstractValidator<DeleteQuestionCommand>
{
    public DeleteQuestionCommandValidator()
    {
        RuleFor(x => x.QuestionId)
            .NotEmpty();
    }
}