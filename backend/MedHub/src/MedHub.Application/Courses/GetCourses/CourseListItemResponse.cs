namespace MedHub.Application.Courses.GetCourses;

public sealed record CourseListItemResponse(
    Guid Id,
    string Title,
    string? Description,
    string Status,
    DateTime CreatedOnUtc,
    int LessonsCount);
