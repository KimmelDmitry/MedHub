namespace MedHub.Domain.Lessons;

public enum LessonStatus
{
    Draft = 1,      // Черновик, виден только преподу
    Published = 2,  // Опубликован, доступен всем студентам
    Archived = 3    // Архивирован (старая версия или удален из доступа)
}