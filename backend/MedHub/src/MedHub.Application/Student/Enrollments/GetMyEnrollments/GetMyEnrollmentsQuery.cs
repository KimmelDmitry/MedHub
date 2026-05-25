using MedHub.Application.Abstractions.Messaging;

namespace MedHub.Application.Student.Enrollments.GetMyEnrollments;

public sealed record GetMyEnrollmentsQuery
    : IQuery<IReadOnlyList<MyEnrollmentResponse>>;
