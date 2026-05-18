using MedHub.Domain.Abstractions;

namespace MedHub.Domain.Checkpoints.ValueObjects;

public sealed record AnswerOptionText
{
    private AnswerOptionText(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<AnswerOptionText> Create(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return Result.Failure<AnswerOptionText>(
                new Error("AnswerOption.TextRequired", "Текст варианта ответа обязателен"));
        }

        var value = text.Trim();

        if (value.Length > 1000)
        {
            return Result.Failure<AnswerOptionText>(
                new Error("AnswerOption.TextTooLong", "Текст варианта ответа слишком длинный"));
        }

        return Result.Success(new AnswerOptionText(value));
    }

    public override string ToString() => Value;
}