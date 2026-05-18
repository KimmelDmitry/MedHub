namespace MedHub.Application.Checkpoints.GetVideoCheckpoints;

public sealed record VideoCheckpointResponse(
    Guid Id,
    int TimestampSeconds,
    int OrderNumber,
    string? Title,
    bool IsRequired,
    bool IsGraded,
    string Status,
    int QuestionsCount
);