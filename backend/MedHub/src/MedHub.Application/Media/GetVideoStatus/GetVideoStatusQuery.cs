using MedHub.Application.Abstractions.Messaging;

namespace MedHub.Application.Media.GetVideoStatus;

public sealed record GetVideoStatusQuery(
    Guid VideoId
) : IQuery<VideoStatusResponse>;