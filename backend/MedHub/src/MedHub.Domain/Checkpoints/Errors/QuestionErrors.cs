using MedHub.Domain.Abstractions;

namespace MedHub.Domain.Checkpoints.Errors;

public static class QuestionErrors
{
    public static readonly Error InvalidText =
        new("Question.InvalidText", "Некорректный текст вопроса");

    public static readonly Error InvalidTimeLimit =
        new("Question.InvalidTimeLimit", "Некорректный лимит времени");

    public static readonly Error InvalidCorrectCount =
        new("Question.InvalidCorrectCount", "Неверное количество правильных ответов");

    public static readonly Error TextQuestionCannotHaveOptions =
        new("Question.TextCannotHaveOptions", "Текстовый вопрос не может иметь варианты ответов");

    public static readonly Error NotEnoughOptions =
        new("Question.NotEnoughOptions", "Недостаточно вариантов ответа");

    public static readonly Error InvalidAnswerShape =
        new("Question.InvalidAnswerShape", "Форма ответа не соответствует типу вопроса");

    public static readonly Error ManualReviewRequired =
        new("Question.ManualReviewRequired", "Для этого вопроса требуется ручная проверка");

    public static readonly Error NotFound =
        new("Question.NotFound", "Вопрос не найден");

    public static readonly Error AnswerOptionNotFound =
        new("Question.AnswerOptionNotFound", "Вариант ответа не был найден");
}