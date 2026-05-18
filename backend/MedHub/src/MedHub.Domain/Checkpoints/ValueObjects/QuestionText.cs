using MedHub.Domain.Abstractions;

namespace MedHub.Domain.Checkpoints.ValueObjects;

public sealed record QuestionText
{
    private QuestionText(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<QuestionText> Create(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return Result.Failure<QuestionText>(
                new Error("Question.TextRequired", "Текст вопроса обязателен"));
        }

        var value = text.Trim();

        if (value.Length > 1000)
        {
            return Result.Failure<QuestionText>(
                new Error("Question.TextTooLong", "Текст вопроса слишком длинный"));
        }

        return Result.Success(new QuestionText(value));
    }

    public override string ToString() => Value;
}