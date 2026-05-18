using MedHub.Domain.Abstractions;

namespace MedHub.Domain.Courses.ValueObjects;

public sealed record CourseDescription
{
    public string Value { get; }

    private  const int MAX_LENGTH = 2000;
    
    private CourseDescription(string value) => Value = value;

    public static readonly Error InvalidLength = new(
        "CourseTitle.InvalidLength", 
        $"Длина описания курса должна быть до {MAX_LENGTH} символов");

    
    public static Result<CourseDescription> Create(string? description)
    {
        if (!string.IsNullOrEmpty(description) && description.Length > MAX_LENGTH)
            return Result.Failure<CourseDescription>(InvalidLength);

        return new CourseDescription(description ?? string.Empty);
    }
}