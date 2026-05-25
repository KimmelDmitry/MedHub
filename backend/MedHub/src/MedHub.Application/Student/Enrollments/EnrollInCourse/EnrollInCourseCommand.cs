using MedHub.Application.Abstractions.Messaging;

namespace MedHub.Application.Student.Enrollments.EnrollInCourse;

public sealed record EnrollInCourseCommand(Guid CourseId)
    : ICommand<EnrollmentResponse>;
