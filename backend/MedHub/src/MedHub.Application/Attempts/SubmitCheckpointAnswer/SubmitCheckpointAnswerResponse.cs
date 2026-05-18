namespace MedHub.Application.Attempts.SubmitCheckpointAnswer;

public sealed record SubmitCheckpointAnswerResponse(
    Guid AnswerId,
    bool IsCorrect,
    decimal CurrentScore
);