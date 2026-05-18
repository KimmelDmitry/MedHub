using MedHub.Application.Abstractions.Messaging;

namespace MedHub.Application.Courses.GetCourseById;

public sealed record GetCourseByIdQuery(
    Guid CourseId
) : IQuery<CourseResponse>;