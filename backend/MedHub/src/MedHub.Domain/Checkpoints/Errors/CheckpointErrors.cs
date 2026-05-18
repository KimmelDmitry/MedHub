using MedHub.Domain.Abstractions;

namespace MedHub.Domain.Checkpoints.Errors;

public static class CheckpointErrors
{
    public static readonly Error NotFound =
        new("Checkpoint.NotFound", "Контрольная точка не найдена");

    public static readonly Error InvalidTimestamp =
        new("Checkpoint.InvalidTimestamp", "Некорректный таймкод");

    public static readonly Error DuplicateTimestamp =
        new("Checkpoint.DuplicateTimestamp", "На этом таймкоде уже есть контрольная точка");

    public static readonly Error InvalidTransition =
        new("Checkpoint.InvalidTransition", "Недопустимый переход состояния");

    public static readonly Error GradedCheckpointRequiresQuestions =
        new("Checkpoint.GradedRequiresQuestions", "Оцениваемая контрольная точка должна содержать хотя бы один вопрос");

    public static readonly Error QuestionNotFound =
        new("Checkpoint.QuestionNotFound", "Вопрос в контрольной точке не найден");

    public static readonly Error InvalidOrderNumber =
        new("Checkpoint.InvalidOrderNumber", "Недопустимый порядковый номер урока в курсе");
}