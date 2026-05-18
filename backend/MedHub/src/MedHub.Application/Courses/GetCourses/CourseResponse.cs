namespace MedHub.Application.Courses.GetCourses;

public sealed class CourseResponse
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public DateTime CreatedAt { get; init; }
    public int LessonsCount { get; init; } // Подсчитаем через SQL
}