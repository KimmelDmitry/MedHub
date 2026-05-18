namespace MedHub.Application.Attempts.StartAttempt;

public sealed record StartAttemptResponse(
    Guid AttemptId,
    Guid LessonId,
    DateTime StartedAt,
    string Status
);