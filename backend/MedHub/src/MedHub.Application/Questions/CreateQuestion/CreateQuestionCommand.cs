using MedHub.Application.Abstractions.Messaging;
using MedHub.Domain.Checkpoints;
using MedHub.Domain.Checkpoints.Enums;

namespace MedHub.Application.Questions.CreateQuestion;

public sealed record CreateQuestionCommand(
    Guid CheckpointId,
    string Text,
    QuestionType Type,
    bool AllowRetry,
    int? TimeLimitSeconds,
    bool RevealCorrectAnswer,
    string? CorrectTextAnswer
) : ICommand<Guid>;