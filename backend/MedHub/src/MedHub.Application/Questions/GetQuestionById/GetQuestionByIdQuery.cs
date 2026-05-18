using MedHub.Application.Abstractions.Messaging;

namespace MedHub.Application.Questions.GetQuestionById;

public sealed record GetQuestionByIdQuery(
    Guid QuestionId
) : IQuery<QuestionResponse>;