using MedHub.Domain.Checkpoints.Enums;

namespace MedHub.Api.Controllers.Questions;

public sealed record CreateQuestionRequest(
    string Text,
    QuestionType Type,
    bool AllowRetry,
    int? TimeLimitSeconds,
    bool RevealCorrectAnswer,
    string? CorrectTextAnswer);