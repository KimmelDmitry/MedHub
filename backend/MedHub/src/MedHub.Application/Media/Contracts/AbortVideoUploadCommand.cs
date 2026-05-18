using MediatR;

namespace MedHub.Application.Media.Contracts;

public sealed record AbortVideoUploadCommand(
    Guid VideoId
) : IRequest<Unit>;