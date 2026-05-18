using MedHub.Application.Abstractions.Messaging;

namespace MedHub.Application.Lessons.UpdateLessonOrder;

public sealed record UpdateLessonOrderCommand(
    Guid LessonId,
    int Order
) : ICommand;