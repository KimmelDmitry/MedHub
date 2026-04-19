using MedHub.Domain.Abstractions;

namespace MedHub.Domain.Lessons.ValueObjects;

public sealed record LessonOrder
{
    public int Value { get; }

    private LessonOrder(int value) => Value = value;

    public static readonly Error InvalidOrder = new(
        "LessonOrder.Invalid",
        "Порядковый номер урока должен быть больше 0");

    public static Result<LessonOrder> Create(int order)
    {
        if (order <= 0)
            return Result.Failure<LessonOrder>(InvalidOrder);

        return new LessonOrder(order);
    }
}