using MedHub.Application.Abstractions.Messaging;

public sealed record AttachVideoToLessonCommand(
    Guid LessonId,
    Guid VideoId)
    : ICommand;