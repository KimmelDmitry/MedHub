namespace MedHub.Application.Courses.GetCourseById;

public sealed record CourseResponse(
    Guid Id,
    string Title,
    string? Description,
    string Status,
    Guid CreatorId,
    DateTime CreatedAt,
    IReadOnlyList<CourseLessonResponse> Lessons
);

public sealed record CourseLessonResponse(
    Guid Id,
    string Title,
    int OrderNumber,
    string Status,
    string ContentType,
    Guid? VideoId
);