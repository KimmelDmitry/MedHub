using MedHub.Domain.Abstractions;
using MediatR;

namespace MedHub.Application.Abstractions.Messaging;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>
{
}