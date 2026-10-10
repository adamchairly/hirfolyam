using System.Text;
using System.Text.Json;
using FluentValidation;
using FluentValidation.Results;
using Hirfolyam.Presentation.Middlewares;
using Microsoft.AspNetCore.Http;

namespace Hirfolyam.Tests.Presentation;

public sealed class ExceptionMiddlewareTests
{
    [Test]
    public async Task ValidationFailureReturns422WithEveryMessage()
    {
        var (context, body) = await InvokeAsync(new ValidationException(
        [
            new ValidationFailure("Page", "The page must be greater than zero."),
            new ValidationFailure("PageSize", "The page size must not exceed 100.")
        ]));

        Assert.That(context.Response.StatusCode, Is.EqualTo(StatusCodes.Status422UnprocessableEntity));
        Assert.That(body.GetProperty("error").GetString(), Is.EqualTo("ValidationError"));
        Assert.That(body.GetProperty("code").GetInt32(), Is.EqualTo(422));
        Assert.That(body.GetProperty("message").GetString(),
            Is.EqualTo("The page must be greater than zero.; The page size must not exceed 100."));
    }

    [Test]
    public async Task InvalidOperationReturns400WithItsMessage()
    {
        var (context, body) = await InvokeAsync(new InvalidOperationException("The request is invalid."));

        Assert.That(context.Response.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
        Assert.That(body.GetProperty("error").GetString(), Is.EqualTo("BadRequest"));
        Assert.That(body.GetProperty("code").GetInt32(), Is.EqualTo(400));
        Assert.That(body.GetProperty("message").GetString(), Is.EqualTo("The request is invalid."));
    }

    [Test]
    public async Task UnexpectedFailureReturns500WithoutDetails()
    {
        var (context, body) = await InvokeAsync(new InvalidDataException("Internal storage failure."));

        Assert.That(context.Response.StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
        Assert.That(body.GetProperty("error").GetString(), Is.EqualTo("InternalError"));
        Assert.That(body.GetProperty("code").GetInt32(), Is.EqualTo(500));
        Assert.That(body.GetProperty("message").ValueKind, Is.EqualTo(JsonValueKind.Null));
    }

    private static async Task<(HttpContext Context, JsonElement Body)> InvokeAsync(Exception exception)
    {
        using var responseBody = new MemoryStream();
        var context = new DefaultHttpContext();
        context.Response.Body = responseBody;

        await new ExceptionMiddleware(_ => throw exception).InvokeAsync(context);

        using var document = JsonDocument.Parse(Encoding.UTF8.GetString(responseBody.ToArray()));

        return (context, document.RootElement.Clone());
    }
}
