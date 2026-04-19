using MedHub.Application.Abstractions.Clock;

namespace MedHub.Infrastructure.Clock;

internal sealed class DateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}