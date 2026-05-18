namespace MedHub.Application.Attempts.GetAttemptResult;

public sealed record AttemptResultResponse(
    Guid AttemptId,
    Guid LessonId,
    decimal Score,
    DateTime StartedAt,
    DateTime? CompletedAt,
    string Status,
    IReadOnlyCollection<AttemptAnswerResponse> Answers
);

public sealed record AttemptAnswerResponse(
    Guid QuestionId,
    bool IsCorrect,
    string? TextAnswer,
    IReadOnlyCollection<Guid> SelectedOptionIds
);