namespace MedHub.Api.Controllers.Checkpoints;

public sealed record CreateCheckpointRequest(
    Guid VideoId,
    int TimestampSeconds,
    int OrderNumber,
    string? Title,
    bool IsRequired,
    bool IsGraded);