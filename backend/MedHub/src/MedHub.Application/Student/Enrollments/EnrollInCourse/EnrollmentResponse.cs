namespace MedHub.Application.Student.Enrollments.EnrollInCourse;

public sealed record EnrollmentResponse(
    Guid EnrollmentId,
    Guid CourseId,
    string Status,
    DateTime EnrolledAtUtc);
