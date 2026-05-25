using MedHub.Domain.Abstractions;

namespace MedHub.Domain.Courses;

public static class CourseErrors
{
    public static Error NotFound => new("Course.NotFound", "Курс не найден");
    public static Error AlreadyPublished => new("Course.AlreadyPublished", "Курс уже опубликован");
    public static Error CannotArchiveDraft => new("Course.CannotArchiveDraft", "Нельзя архивировать черновик, сначала удалите его");

    public static Error NoLessons => new("Course.NoLessons", "нельзя публиковать курс без уроков");
}
