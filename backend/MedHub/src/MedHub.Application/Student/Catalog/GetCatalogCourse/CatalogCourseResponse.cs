namespace MedHub.Application.Student.Catalog.GetCatalogCourse;

public sealed record CatalogCourseResponse(
    Guid Id,
    string Title,
    string? Description,
    bool IsEnrolled,
    string? EnrollmentStatus,
    IReadOnlyList<CatalogLessonItemResponse> Lessons);

public sealed record CatalogLessonItemResponse(
    Guid Id,
    string Title,
    int Order,
    string ContentType,
    bool HasVideo,
    bool VideoReady,
    int? DurationSeconds,
    int CheckpointsCount);
