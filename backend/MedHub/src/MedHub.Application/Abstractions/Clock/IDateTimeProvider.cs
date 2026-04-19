namespace MedHub.Application.Abstractions.Clock;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}