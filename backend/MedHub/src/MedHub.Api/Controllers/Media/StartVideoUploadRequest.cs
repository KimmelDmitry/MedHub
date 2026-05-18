namespace MedHub.Api.Controllers.Media;

public sealed record StartVideoUploadRequest(
    Guid LessonId,
    string FileName,
    string ContentType,
    long SizeBytes
);