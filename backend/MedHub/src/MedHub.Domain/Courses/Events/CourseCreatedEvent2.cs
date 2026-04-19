using MedHub.Domain.Abstractions;

public sealed record CoursePublishedEvent(Guid CourseId) : IDomainEvent;