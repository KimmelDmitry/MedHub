using MedHub.Application.Abstractions.Messaging;

namespace MedHub.Application.Courses.UpdateCourseDescription;

public sealed record UpdateCourseDescriptionCommand(
    Guid CourseId,
    string? Description
) : ICommand;