using MedHub.Domain.Checkpoints.Enums;

namespace MedHub.Application.Questions.GetCheckpointQuestions;

public sealed record CheckpointQuestionResponse(
    Guid Id,
    string Text,
    QuestionType Type,
    int OptionsCount
);