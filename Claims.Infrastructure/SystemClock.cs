using Claims.Application.Abstractions;

namespace Claims.Infrastructure;

public sealed class SystemClock : ISystemClock
{
    public DateOnly Today => DateOnly.FromDateTime(DateTime.Today);
}
