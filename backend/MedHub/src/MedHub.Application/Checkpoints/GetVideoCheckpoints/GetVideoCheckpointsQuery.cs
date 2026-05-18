using MedHub.Application.Abstractions.Messaging;

namespace MedHub.Application.Checkpoints.GetVideoCheckpoints;

public sealed record GetVideoCheckpointsQuery(
    Guid VideoId
) : IQuery<IReadOnlyList<VideoCheckpointResponse>>;