using MedHub.Domain.Abstractions;

namespace MedHub.Domain.Enrollments.Events;

public sealed record EnrollmentCompletedEvent(Guid EnrollmentId) : IDomainEvent;
