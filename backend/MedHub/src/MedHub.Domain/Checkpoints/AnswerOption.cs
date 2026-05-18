using MedHub.Domain.Abstractions;
using MedHub.Domain.Checkpoints.Events;
using MedHub.Domain.Checkpoints.ValueObjects;

namespace MedHub.Domain.Checkpoints;

public sealed class AnswerOption : Entity
{
    private AnswerOption()
    {
    }

    internal AnswerOption(
        Guid id,
        Guid questionId,
        AnswerOptionText text,
        bool isCorrect)
        : base(id)
    {
        QuestionId = questionId;
        Text = text;
        IsCorrect = isCorrect;
    }

    public Guid QuestionId { get; private set; }
    public AnswerOptionText Text { get; private set; } = null!;
    public bool IsCorrect { get; private set; }

    public Result UpdateText(string newText)
    {
        Result<AnswerOptionText> textResult = AnswerOptionText.Create(newText);
        if (textResult.IsFailure)
        {
            return Result.Failure(textResult.Error);
        }

        Text = textResult.Value;
        return Result.Success();
    }

    public void SetCorrectness(bool isCorrect)
    {
        IsCorrect = isCorrect;
    }

    internal void Update(AnswerOptionText newText, bool newIsCorrect)
    {
        Text = newText;
        IsCorrect = newIsCorrect;
    }
}