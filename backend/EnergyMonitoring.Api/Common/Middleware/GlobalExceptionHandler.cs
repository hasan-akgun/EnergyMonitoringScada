using EnergyMonitoring.Api.Common.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace EnergyMonitoring.Api.Common.Middleware;

public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var problemDetails = CreateProblemDetails(exception);

        problemDetails.Instance = httpContext.Request.Path;

        problemDetails.Extensions["traceId"] =
            httpContext.TraceIdentifier;

        httpContext.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;

        LogException(
            exception,
            httpContext.Response.StatusCode,
            httpContext.TraceIdentifier);

        var wasWritten = await problemDetailsService.TryWriteAsync(
            new ProblemDetailsContext
            {
                HttpContext = httpContext,
                ProblemDetails = problemDetails,
                Exception = exception
            });

        if (!wasWritten)
        {
            await httpContext.Response.WriteAsJsonAsync(
                problemDetails,
                cancellationToken);
        }

        return true;
    }

    private static ProblemDetails CreateProblemDetails(Exception exception)
    {
        return exception switch
        {
            ValidationException validationException =>
                new ValidationProblemDetails(
                    validationException.Errors.ToDictionary(
                        x => x.Key,
                        x => x.Value))
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Validation failed",
                    Detail = validationException.Message
                },

            ArgumentException argumentException =>
                new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Invalid request",
                    Detail = argumentException.Message
                },

            KeyNotFoundException keyNotFoundException =>
                new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Resource not found",
                    Detail = keyNotFoundException.Message
                },

            UnauthorizedAccessException =>
                new ProblemDetails
                {
                    Status = StatusCodes.Status403Forbidden,
                    Title = "Access denied",
                    Detail = "Bu işlemi gerçekleştirmek için yetkiniz yok."
                },

            _ =>
                new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "Internal server error",
                    Detail = "İstek işlenirken beklenmeyen bir hata oluştu."
                }
        };
    }

    private void LogException(
        Exception exception,
        int statusCode,
        string traceId)
    {
        if (statusCode >= 500)
        {
            logger.LogError(
                exception,
                "Unhandled exception. TraceId: {TraceId}",
                traceId);

            return;
        }

        logger.LogWarning(
            "Request rejected. StatusCode: {StatusCode}, " +
            "Message: {Message}, TraceId: {TraceId}",
            statusCode,
            exception.Message,
            traceId);
    }
}