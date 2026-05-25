using MedHub.Domain.Abstractions;

namespace MedHub.Domain.Lessons;

public static class LessonErrors
{
    public static Error NotFound => new("Lesson.NotFound", "Урок не найден");
    public static Error AlreadyPublished => new("Lesson.AlreadyPublished", "Урок уже опубликован");
    
    public static Error InvalidStatusTransition => new(
        "Lesson.InvalidStatusTransition", 
        "Недопустимый переход статуса урока");

    public static Error CourseMismatch => new(
        "Lesson.CourseMismatch", 
        "Урок принадлежит другому курсу");
    
    public static Error EmptyUrl =>  new("Lesson.EmptyUrl", "В урл прокинули пустую строку, низя");
    
    public static Error NoContent => new("Lesson.NoContent", "нельзя публиковать урок без контента");
}
