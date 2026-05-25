using MedHub.Application.Abstractions.Messaging;

namespace MedHub.Application.Student.Lessons.GetStudentLessonRuntime;

public sealed record GetStudentLessonRuntimeQuery(Guid LessonId)
    : IQuery<StudentLessonRuntimeResponse>;
