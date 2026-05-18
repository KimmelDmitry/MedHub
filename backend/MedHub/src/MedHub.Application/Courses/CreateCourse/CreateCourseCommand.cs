using MedHub.Application.Abstractions.Messaging;

namespace MedHub.Application.Courses.CreateCourse;

public sealed record CreateCourseCommand(
    string Title,
    string? Description) : ICommand<Guid>;