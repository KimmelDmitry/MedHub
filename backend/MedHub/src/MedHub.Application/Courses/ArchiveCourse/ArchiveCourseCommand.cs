using MedHub.Application.Abstractions.Messaging;

namespace MedHub.Application.Courses.ArchiveCourse;

public sealed record ArchiveCourseCommand(
    Guid CourseId
) : ICommand;