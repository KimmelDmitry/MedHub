namespace MedHub.Domain.Checkpoints.Models;

public sealed record QuestionEvaluationResult(
    bool IsCorrect,
    bool RequiresManualReview
);