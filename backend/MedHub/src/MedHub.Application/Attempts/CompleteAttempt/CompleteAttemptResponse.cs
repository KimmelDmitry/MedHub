namespace MedHub.Application.Attempts.CompleteAttempt;

public sealed record CompleteAttemptResponse(
    Guid AttemptId,
    decimal FinalScore,
    DateTime CompletedAt,
    string Status
);