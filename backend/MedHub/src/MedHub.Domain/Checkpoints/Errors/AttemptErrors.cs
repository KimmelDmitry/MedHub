using MedHub.Domain.Abstractions;

namespace MedHub.Domain.Checkpoints.Errors;

public static class AttemptErrors
{
    public static readonly Error NotFound =
        new("Attempt.NotFound", "Попытка не найдена");

    public static readonly Error InvalidTransition =
        new("Attempt.InvalidTransition", "Недопустимое состояние попытки");

    public static readonly Error AlreadyAnswered =
        new("Attempt.AlreadyAnswered", "На этот вопрос уже есть ответ");

    public static readonly Error QuestionMismatch =
        new("Attempt.QuestionMismatch", "Вопрос не принадлежит текущему уроку");

    public static readonly Error AlreadyCompleted =
        new("Attempt.AlreadyCompleted", "Попытка уже завершена");

    public static readonly Error InvalidScore =
        new("Attempt.InvalidScore", "Некорректный балл");

    public static readonly Error InvalidStudentId = 
        new("Attempt.InvalidStudentId", "Идентификатор студента не может быть пустым.");
    
    public static readonly Error InvalidLessonId  
        = new("Attempt.InvalidLessonId", "Идентификатор урока не может быть пустым.");
}