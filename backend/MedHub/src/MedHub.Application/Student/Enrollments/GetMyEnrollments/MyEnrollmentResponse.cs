namespace MedHub.Application.Student.Enrollments.GetMyEnrollments;

public sealed record MyEnrollmentResponse(
    Guid EnrollmentId,
    Guid CourseId,
    string CourseTitle,
    string? CourseDescription,
    string Status,
    DateTime EnrolledAtUtc,
    DateTime? CompletedAtUtc,
    int LessonsCount,
    int CompletedLessonsCount,
    DateTime? LastAttemptAt);
