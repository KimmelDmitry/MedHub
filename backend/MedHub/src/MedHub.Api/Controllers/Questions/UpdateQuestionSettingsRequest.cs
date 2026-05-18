namespace MedHub.Api.Controllers.Questions;

public sealed record UpdateQuestionSettingsRequest(
    bool AllowRetry,
    int? TimeLimitSeconds,
    bool RevealCorrectAnswer,
    string? CorrectTextAnswer);