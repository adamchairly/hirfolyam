using System.Text;
using System.Text.Json;
using FluentValidation;
using FluentValidation.Results;
using Hirfolyam.Application.Shared;
using Hirfolyam.Presentation.Middlewares;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;

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
    public async Task MissingResourceReturns404WithItsMessage()
    {
        var (context, body) = await InvokeAsync(new NotFoundException("Hot topic 5 was not found."));

        Assert.That(context.Response.StatusCode, Is.EqualTo(StatusCodes.Status404NotFound));
        Assert.That(body.GetProperty("error").GetString(), Is.EqualTo("NotFound"));
        Assert.That(body.GetProperty("code").GetInt32(), Is.EqualTo(404));
        Assert.That(body.GetProperty("message").GetString(), Is.EqualTo("Hot topic 5 was not found."));
    }

    // An InvalidOperationException is an internal failure too, for example from EF Core.
    [TestCase(typeof(InvalidDataException))]
    [TestCase(typeof(InvalidOperationException))]
    public async Task UnexpectedFailureReturns500WithoutDetails(Type exceptionType)
    {
        var exception = (Exception)Activator.CreateInstance(exceptionType, "Internal storage failure.")!;
        var (context, body) = await InvokeAsync(exception);

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

        await new ExceptionMiddleware(_ => throw exception, NullLogger<ExceptionMiddleware>.Instance).InvokeAsync(context);

        using var document = JsonDocument.Parse(Encoding.UTF8.GetString(responseBody.ToArray()));

        return (context, document.RootElement.Clone());
    }
}
