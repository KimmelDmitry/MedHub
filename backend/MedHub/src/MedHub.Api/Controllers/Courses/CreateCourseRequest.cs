namespace MedHub.Api.Controllers.Courses;

public sealed record CreateCourseRequest(
    string Title,
    string? Description
);