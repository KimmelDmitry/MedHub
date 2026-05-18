using MedHub.Application.Abstractions.Messaging;
using MedHub.Domain.Lessons;

namespace MedHub.Application.Lessons.CreateLesson;

public sealed record CreateLessonCommand(
    Guid CourseId,
    string Title,
    int Order,
    LessonContentType ContentType,
    string? ContentUrl = null) : ICommand<Guid>;