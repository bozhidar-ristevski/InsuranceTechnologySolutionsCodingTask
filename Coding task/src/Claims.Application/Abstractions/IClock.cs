namespace Claims.Application.Abstractions;

public interface IClock
{
    DateTime UtcNow { get; }
}
