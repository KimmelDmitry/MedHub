using MedHub.Application.Abstractions.Messaging;
using MedHub.Domain.Lessons;

namespace MedHub.Application.Lessons.UpdateLessonContent;

public sealed record UpdateLessonContentCommand(
    Guid LessonId,
    string ContentUrl,
    LessonContentType ContentType
) : ICommand;