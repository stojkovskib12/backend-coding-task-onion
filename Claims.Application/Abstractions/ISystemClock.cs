namespace Claims.Application.Abstractions;

public interface ISystemClock
{
    DateOnly Today { get; }
}
