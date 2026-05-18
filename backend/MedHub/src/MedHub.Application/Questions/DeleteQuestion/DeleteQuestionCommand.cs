using MedHub.Application.Abstractions.Messaging;

namespace MedHub.Application.Questions.DeleteQuestion;

public sealed record DeleteQuestionCommand(
    Guid QuestionId
) : ICommand;