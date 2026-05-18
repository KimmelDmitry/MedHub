using MedHub.Application.Abstractions.Messaging;

namespace MedHub.Application.Lessons.UpdateLessonTitle;

public sealed record UpdateLessonTitleCommand(
    Guid LessonId,
    string Title
) : ICommand;