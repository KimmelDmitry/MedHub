using MedHub.Domain.Abstractions;

namespace MedHub.Domain.Courses.ValueObjects;

public sealed record CourseTitle
{
    public string Value { get; }

    private CourseTitle(string value) => Value = value;

    public static readonly Error InvalidLength = new(
        "CourseTitle.InvalidLength", 
        "Длина названия курса должна быть от 3 до 100 символов");

    public static Result<CourseTitle> Create(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            return Result.Failure<CourseTitle>(Error.NullValue);

        if (title.Length < 3 || title.Length > 100)
            return Result.Failure<CourseTitle>(InvalidLength);

        return new CourseTitle(title.Trim());
    }
}