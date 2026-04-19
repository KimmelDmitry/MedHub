using MedHub.Domain.Abstractions;

namespace MedHub.Domain.Courses.Events;


public sealed record CourseCreatedEvent(Guid CourseId, Guid TeacherId) : IDomainEvent;