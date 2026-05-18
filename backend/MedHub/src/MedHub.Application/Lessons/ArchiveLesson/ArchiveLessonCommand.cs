using MedHub.Application.Abstractions.Messaging;

namespace MedHub.Application.Lessons.ArchiveLesson;

public sealed record ArchiveLessonCommand(
    Guid LessonId
) : ICommand;