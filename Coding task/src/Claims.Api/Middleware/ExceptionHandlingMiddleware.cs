using Claims.Application.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Claims.Api.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private const string ProblemJson = "application/problem+json";

    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException exception)
        {
            await WriteProblemAsync(
                context,
                new ValidationProblemDetails(new Dictionary<string, string[]>
                {
                    [string.Empty] = exception.Errors.ToArray()
                })
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "One or more validation errors occurred.",
                    Detail = string.Join("; ", exception.Errors),
                    Instance = context.Request.Path
                });
        }
        catch (NotFoundException exception)
        {
            await WriteProblemAsync(
                context,
                new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Not Found",
                    Detail = exception.Message,
                    Instance = context.Request.Path
                });
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unhandled exception");
            await WriteProblemAsync(
                context,
                new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "An unexpected error occurred.",
                    Detail = "An unexpected error occurred.",
                    Instance = context.Request.Path
                });
        }
    }

    private static async Task WriteProblemAsync(HttpContext context, ProblemDetails problem)
    {
        context.Response.StatusCode = problem.Status ?? StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsJsonAsync(problem, options: null, contentType: ProblemJson);
    }
}
