using MedHub.Application.Abstractions.Messaging;

namespace MedHub.Application.Questions.GetCheckpointQuestions;

public sealed record GetCheckpointQuestionsQuery(
    Guid CheckpointId
) : IQuery<IReadOnlyList<CheckpointQuestionResponse>>;