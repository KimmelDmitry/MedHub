using MedHub.Domain.Abstractions;

public sealed record CourseArchivedEvent(Guid CourseId) : IDomainEvent;