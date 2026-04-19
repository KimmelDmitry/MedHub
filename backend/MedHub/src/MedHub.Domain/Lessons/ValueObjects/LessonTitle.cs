using MedHub.Domain.Abstractions;

namespace MedHub.Domain.Lessons.ValueObjects;

public sealed record LessonTitle
{
    public string Value { get; }

    private LessonTitle(string value) => Value = value;

    public static readonly Error InvalidLength = new(
        "LessonTitle.InvalidLength",
        "Длина названия урока должна быть от 3 до 150 символов");

    public static Result<LessonTitle> Create(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            return Result.Failure<LessonTitle>(Error.NullValue);

        if (title.Length < 3 || title.Length > 150)
            return Result.Failure<LessonTitle>(InvalidLength);

        return new LessonTitle(title.Trim());
    }
}