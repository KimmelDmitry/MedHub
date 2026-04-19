namespace MedHub.Domain.Courses.ValueObjects;

public record CourseDescription
{
    public string Value { get; }

    private CourseDescription(string value) => Value = value ?? string.Empty;

    public static CourseDescription Create(string? description)
    {
        if (!string.IsNullOrEmpty(description) && description.Length > 2000)
            throw new ApplicationException("Описание слишком длинное");

        return new CourseDescription(description ?? string.Empty);
    }
}