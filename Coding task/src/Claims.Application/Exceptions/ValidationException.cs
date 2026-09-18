namespace Claims.Application.Exceptions;

public sealed class ValidationException : Exception
{
    public ValidationException(params string[] errors)
        : base(errors.Length == 1 ? errors[0] : "One or more validation errors occurred.")
    {
        Errors = errors;
    }

    public IReadOnlyList<string> Errors { get; }
}
