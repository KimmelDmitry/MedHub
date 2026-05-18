using MedHub.Domain.Lessons;

namespace MedHub.Application.Lessons.GetLessonsByCourse;

public sealed class LessonResponse
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public int Order { get; init; }
    public LessonContentType ContentType { get; init; }
    public bool HasVideo { get; init; } // Флаг: есть ли видео у этого урока
}