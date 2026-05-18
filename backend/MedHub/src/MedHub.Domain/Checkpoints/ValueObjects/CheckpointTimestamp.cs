using MedHub.Domain.Abstractions;

namespace MedHub.Domain.Checkpoints.ValueObjects;

public sealed record CheckpointTimestamp
{
    private CheckpointTimestamp(int value)
    {
        Value = value;
    }

    public int Value { get; }

    public static Result<CheckpointTimestamp> Create(int seconds, int videoDurationSeconds)
    {
        if (seconds < 0)
        {
            return Result.Failure<CheckpointTimestamp>(
                new Error("Checkpoint.TimestampNegative", "Таймкод не может быть отрицательным"));
        }

        if (videoDurationSeconds > 0 && seconds > videoDurationSeconds)
        {
            return Result.Failure<CheckpointTimestamp>(
                new Error("Checkpoint.TimestampOutOfRange", "Таймкод выходит за пределы видео"));
        }

        return Result.Success(new CheckpointTimestamp(seconds));
    }

    public override string ToString() => Value.ToString();
}