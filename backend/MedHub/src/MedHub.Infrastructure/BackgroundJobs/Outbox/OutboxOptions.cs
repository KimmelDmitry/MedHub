namespace MedHub.Infrastructure.BackgroundJobs.Outbox;

public sealed class OutboxOptions
{
    public int IntervalInSeconds { get; init; }

    public int BatchSize { get; init; }
}
