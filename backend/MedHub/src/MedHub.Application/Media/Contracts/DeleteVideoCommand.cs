using MediatR;

namespace MedHub.Application.Media.Contracts;


public sealed record DeleteVideoCommand(
    Guid VideoId
) : IRequest<Unit>;