using MedHub.Domain.Abstractions;

namespace MedHub.Domain.Enrollments.Events;

public sealed record EnrollmentCancelledEvent(Guid EnrollmentId) : IDomainEvent;
