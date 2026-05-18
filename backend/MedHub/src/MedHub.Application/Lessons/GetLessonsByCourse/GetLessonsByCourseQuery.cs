using MedHub.Application.Abstractions.Messaging;

namespace MedHub.Application.Lessons.GetLessonsByCourse;

public sealed record GetLessonsByCourseQuery(Guid CourseId) 
    : IQuery<IReadOnlyList<LessonResponse>>;