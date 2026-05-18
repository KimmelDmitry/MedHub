using MedHub.Domain.Abstractions;

namespace MedHub.Domain.Checkpoints.Events;

public record AnswerOptionUpdatedEvent(Guid Id, Guid  answerOptionId) : IDomainEvent;