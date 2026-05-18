using MedHub.Domain.Lessons;

namespace MedHub.Application.Lessons;

public sealed record LessonResponse(
    Guid Id,
    Guid CourseId,
    string Title,
    int OrderNumber,
    string ContentType,
    string ContentUrl,
    string Status,
    Guid? VideoId,
    DateTime CreatedAt
);