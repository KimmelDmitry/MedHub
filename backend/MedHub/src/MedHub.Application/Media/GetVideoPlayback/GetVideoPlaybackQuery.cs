using MedHub.Application.Abstractions.Messaging;

namespace MedHub.Application.Media.GetVideoPlayback;

public sealed record GetVideoPlaybackQuery(
    Guid VideoId
) : IQuery<VideoPlaybackResponse>;