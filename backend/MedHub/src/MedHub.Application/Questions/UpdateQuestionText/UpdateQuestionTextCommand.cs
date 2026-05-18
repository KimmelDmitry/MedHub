using MedHub.Application.Abstractions.Messaging;

namespace MedHub.Application.Questions.UpdateQuestionText;

public sealed record UpdateQuestionTextCommand(
    Guid QuestionId,
    string Text
) : ICommand;