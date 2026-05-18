using MedHub.Application.Abstractions.Messaging;

namespace MedHub.Application.Media.StartVideoUpload;

public sealed record StartVideoUploadCommand(
    Guid LessonId,
    string FileName,
    string ContentType,
    long SizeBytes
) : ICommand<StartVideoUploadResult>;