using MedHub.Application.Abstractions.Messaging;

namespace MedHub.Application.Questions.UpdateAnswerOption;

public sealed record UpdateAnswerOptionCommand(
    Guid QuestionId,
    Guid AnswerOptionId,
    string Text,
    bool IsCorrect
) : ICommand;