using MedHub.Application.Abstractions.Messaging;

namespace MedHub.Application.Attempts.SubmitCheckpointAnswer;

public sealed record SubmitCheckpointAnswerCommand(
    Guid AttemptId,
    Guid QuestionId,
    IReadOnlyList<Guid> SelectedOptionIds,
    string? TextAnswer
) : ICommand<SubmitCheckpointAnswerResponse>;