using MedHub.Application.Abstractions.Messaging;

namespace MedHub.Application.Questions.RemoveAnswerOption;

public sealed record RemoveAnswerOptionCommand(
    Guid QuestionId,
    Guid AnswerOptionId
) : ICommand;