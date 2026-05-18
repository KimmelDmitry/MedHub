using MedHub.Application.Abstractions.Messaging;

namespace MedHub.Application.Courses.UpdateCourseTitle;

public sealed record UpdateCourseTitleCommand(
    Guid CourseId,
    string Title
) : ICommand;