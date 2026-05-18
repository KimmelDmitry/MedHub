using MedHub.Application.Abstractions.Messaging;

namespace MedHub.Application.Media.LinkVideoToLesson;

public sealed record LinkVideoToLessonCommand(
    Guid LessonId,
    Guid VideoId
) : ICommand;