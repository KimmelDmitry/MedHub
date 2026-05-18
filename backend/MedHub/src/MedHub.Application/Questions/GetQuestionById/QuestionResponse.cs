using MedHub.Domain.Checkpoints.Enums;

namespace MedHub.Application.Questions.GetQuestionById;

public sealed record QuestionResponse(
    Guid Id,
    Guid CheckpointId,
    string Text,
    QuestionType Type,
    bool AllowRetry,
    int? TimeLimitSeconds,
    bool RevealCorrectAnswer,
    string? CorrectTextAnswer,
    IReadOnlyList<AnswerOptionResponse> AnswerOptions
);

public sealed record AnswerOptionResponse(
    Guid Id,
    string Text,
    bool IsCorrect
);