using MedHub.Application.Abstractions.Messaging;

namespace MedHub.Application.Lessons.GetLessonById;

public sealed record GetLessonByIdQuery(Guid LessonId)
    : IQuery<LessonResponse>;