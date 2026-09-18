namespace Claims.Application.Exceptions;

public sealed class NotFoundException : Exception
{
    public NotFoundException(string name, object key)
        : base($"{name} '{key}' was not found.")
    {
    }
}
