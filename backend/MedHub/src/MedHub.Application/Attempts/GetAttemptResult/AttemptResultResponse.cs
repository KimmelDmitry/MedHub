namespace MedHub.Application.Attempts.GetAttemptResult;

public sealed record AttemptResultResponse(
    Guid AttemptId,
    Guid LessonId,
    string Status,
    decimal Score,
    DateTime StartedAt,
    DateTime? CompletedAt,
    int TotalQuestions,
    int CorrectAnswers,
    IReadOnlyCollection<AttemptAnswerReviewResponse> Answers
);

public sealed record AttemptAnswerReviewResponse(
    Guid QuestionId,
    Guid CheckpointId,
    string? CheckpointTitle,
    int TimestampSeconds,
    string QuestionText,
    string Type,
    IReadOnlyCollection<AnswerOptionReviewResponse> SelectedOptions,
    bool IsCorrect,
    bool RevealCorrectAnswer,
    IReadOnlyCollection<AnswerOptionReviewResponse> CorrectOptions,
    string? TextAnswer,
    bool RequiresManualReview
);

public sealed record AnswerOptionReviewResponse(
    Guid Id,
    string Text
);
