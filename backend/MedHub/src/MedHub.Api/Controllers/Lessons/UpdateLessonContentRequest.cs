using MedHub.Domain.Lessons;

namespace MedHub.Api.Controllers.Lessons;

public sealed record UpdateLessonContentRequest(
    string ContentUrl,
    LessonContentType ContentType
);