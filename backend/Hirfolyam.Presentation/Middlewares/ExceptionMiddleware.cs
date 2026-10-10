using FluentValidation;
using Hirfolyam.Application.Shared;

namespace Hirfolyam.Presentation.Middlewares;

public sealed class ExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            if (!context.Response.HasStarted)
            {
                context.Response.StatusCode = StatusCodes.Status499ClientClosedRequest;
            }
        }
        catch (ValidationException exception) when (!context.Response.HasStarted)
        {
            context.Response.Clear();
            context.Response.StatusCode = StatusCodes.Status422UnprocessableEntity;

            await context.Response.WriteAsJsonAsync(new BaseResponse
            {
                Error = ErrorCodes.ValidationError,
                Message = string.Join("; ", exception.Errors.Select(error => error.ErrorMessage)),
                Code = 422
            });
        }
        catch (InvalidOperationException exception) when (!context.Response.HasStarted)
        {
            context.Response.Clear();
            context.Response.StatusCode = StatusCodes.Status400BadRequest;

            await context.Response.WriteAsJsonAsync(new BaseResponse
            {
                Error = ErrorCodes.BadRequest,
                Message = exception.Message,
                Code = 400
            });
        }
        catch (Exception) when (!context.Response.HasStarted)
        {
            context.Response.Clear();
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;

            await context.Response.WriteAsJsonAsync(new BaseResponse
            {
                Error = ErrorCodes.InternalError,
                Code = 500
            });
        }
    }
}
