using MedHub.Application.Abstractions.Messaging;

namespace MedHub.Application.Courses.PublishCourse;

public sealed record PublishCourseCommand(
    Guid CourseId
) : ICommand;