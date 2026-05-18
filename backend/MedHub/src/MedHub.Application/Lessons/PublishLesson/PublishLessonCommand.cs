using MedHub.Application.Abstractions.Messaging;

namespace MedHub.Application.Lessons.PublishLesson;

public sealed record PublishLessonCommand(Guid LessonId)
    : ICommand;