using MedHub.Domain.Abstractions;

namespace MedHub.Domain.Courses.ValueObjects;

public sealed record CourseTitle
{
    public string Value { get; }
    
    private const int MIN_LENGTH = 3;
    private const int MAX_LENGTH = 100;

    private CourseTitle(string value) => Value = value ?? string.Empty;

    public static readonly Error InvalidLength = new(
        "CourseTitle.InvalidLength", 
        $"Длина названия курса должна быть от {MIN_LENGTH} до {MAX_LENGTH} символов");

    public static Result<CourseTitle> Create(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            return Result.Failure<CourseTitle>(Error.NullValue);

        if (title.Length < MIN_LENGTH || title.Length > MAX_LENGTH)
            return Result.Failure<CourseTitle>(InvalidLength);

        return new CourseTitle(title.Trim());
    }
}