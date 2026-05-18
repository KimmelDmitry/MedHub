using MedHub.Domain.Lessons;

namespace MedHub.Api.Controllers.Lessons;

public sealed record CreateLessonRequest(
    Guid CourseId,
    string Title,
    int Order,
    LessonContentType ContentType,
    string? ContentUrl
);