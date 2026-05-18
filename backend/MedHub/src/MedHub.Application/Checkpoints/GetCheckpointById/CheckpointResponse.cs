namespace MedHub.Application.Checkpoints.GetCheckpointById;

public sealed record CheckpointResponse(
    Guid Id,
    Guid VideoId,
    int TimestampSeconds,
    int OrderNumber,
    string? Title,
    bool IsRequired,
    bool IsGraded,
    string Status,
    IReadOnlyList<CheckpointQuestionResponse> Questions
);

public sealed record CheckpointQuestionResponse(
    Guid Id,
    string Text,
    string Type,
    bool AllowRetry,
    int? TimeLimitSeconds,
    bool RevealCorrectAnswer,
    string? CorrectTextAnswer,
    IReadOnlyList<CheckpointAnswerOptionResponse> AnswerOptions
);

public sealed record CheckpointAnswerOptionResponse(
    Guid Id,
    string Text,
    bool IsCorrect
);