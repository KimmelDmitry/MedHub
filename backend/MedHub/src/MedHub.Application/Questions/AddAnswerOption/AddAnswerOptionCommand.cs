using MedHub.Application.Abstractions.Messaging;

namespace MedHub.Application.Questions.AddAnswerOption;

public sealed record AddAnswerOptionCommand(
    Guid QuestionId,
    string Text,
    bool IsCorrect
) : ICommand<Guid>;