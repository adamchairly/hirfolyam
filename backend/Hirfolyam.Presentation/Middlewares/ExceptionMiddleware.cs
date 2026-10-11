using FluentValidation;
using Hirfolyam.Application.Shared;

namespace Hirfolyam.Presentation.Middlewares;

public sealed class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            if (!context.Response.HasStarted)
            {
                context.Response.StatusCode = StatusCodes.Status499ClientClosedRequest;
            }

            return;
        }
        catch (ValidationException exception) when (!context.Response.HasStarted)
        {
            await WriteAsync(context, new ErrorResponse
            {
                Error = ErrorCodes.ValidationError,
                Message = string.Join("; ", exception.Errors.Select(error => error.ErrorMessage)),
                Code = StatusCodes.Status422UnprocessableEntity
            });
        }
        catch (NotFoundException exception) when (!context.Response.HasStarted)
        {
            await WriteAsync(context, new ErrorResponse
            {
                Error = ErrorCodes.NotFound,
                Message = exception.Message,
                Code = StatusCodes.Status404NotFound
            });
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unhandled exception while processing {Method} {Path}",
                context.Request.Method, context.Request.Path);

            if (context.Response.HasStarted)
            {
                throw;
            }

            // Internal details are only logged.
            await WriteAsync(context, new ErrorResponse
            {
                Error = ErrorCodes.InternalError,
                Code = StatusCodes.Status500InternalServerError
            });

            return;
        }

        if (context.Response.StatusCode is not (StatusCodes.Status200OK or StatusCodes.Status404NotFound))
        {
            logger.LogInformation("HTTP {Method} {Path} responded {StatusCode}", context.Request.Method,
                context.Request.Path, context.Response.StatusCode);
        }
    }

    private static Task WriteAsync(HttpContext context, ErrorResponse response)
    {
        context.Response.Clear();
        context.Response.StatusCode = response.Code!.Value;

        return context.Response.WriteAsJsonAsync(response);
    }
}
