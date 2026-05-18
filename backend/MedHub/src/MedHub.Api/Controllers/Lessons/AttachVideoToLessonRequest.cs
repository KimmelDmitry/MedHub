namespace MedHub.Api.Controllers.Lessons;

public sealed record AttachVideoToLessonRequest(
    Guid VideoId
);