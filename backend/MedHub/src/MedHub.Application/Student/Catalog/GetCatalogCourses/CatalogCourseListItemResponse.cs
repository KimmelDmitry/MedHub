namespace MedHub.Application.Student.Catalog.GetCatalogCourses;

public sealed record CatalogCourseListItemResponse(
    Guid Id,
    string Title,
    string? Description,
    int LessonsCount,
    int PublishedLessonsCount,
    bool HasVideo,
    int CheckpointsCount,
    DateTime CreatedAt,
    bool IsEnrolled,
    string? EnrollmentStatus);
