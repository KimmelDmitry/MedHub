using MedHub.Domain.Abstractions;

namespace MedHub.Domain.Users.Events;

public sealed record UserCreatedDomainEvent(Guid UserId) : IDomainEvent;