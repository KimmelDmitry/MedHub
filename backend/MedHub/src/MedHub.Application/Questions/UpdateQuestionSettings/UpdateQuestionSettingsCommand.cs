using MedHub.Application.Abstractions.Messaging;

namespace MedHub.Application.Questions.UpdateQuestionSettings;

public sealed record UpdateQuestionSettingsCommand(
    Guid QuestionId,
    bool AllowRetry,
    int? TimeLimitSeconds,
    bool RevealCorrectAnswer,
    string? CorrectTextAnswer
) : ICommand;