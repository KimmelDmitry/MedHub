using MediatR;

namespace MedHub.Application.Media.Contracts;

public sealed record GetVideoStatusQuery(
    Guid VideoId
) : IRequest<VideoStatusDto?>;