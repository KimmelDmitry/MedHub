using MedHub.Application.Abstractions.Messaging;

namespace MedHub.Application.Attempts.GetActiveAttemptForLesson;

public sealed record GetActiveAttemptForLessonQuery(
    Guid LessonId)
    : IQuery<ActiveAttemptResponse>;