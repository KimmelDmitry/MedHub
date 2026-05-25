using MedHub.Application.Abstractions.Messaging;

namespace MedHub.Application.Courses.GetCourses;

public sealed record GetCoursesQuery : IQuery<IReadOnlyList<CourseListItemResponse>>;
