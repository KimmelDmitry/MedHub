using MedHub.Domain.Abstractions;

namespace MedHub.Domain.Enrollments.Events;

public sealed record EnrollmentCreatedEvent(
    Guid EnrollmentId,
    Guid StudentId,
    Guid CourseId) : IDomainEvent;
