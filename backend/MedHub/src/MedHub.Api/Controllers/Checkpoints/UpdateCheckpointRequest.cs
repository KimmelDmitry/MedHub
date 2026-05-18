namespace MedHub.Api.Controllers.Checkpoints;

public sealed record UpdateCheckpointRequest(
    string? Title,
    int? TimestampSeconds,
    int? OrderNumber,
    bool? IsRequired,
    bool? IsGraded);