using Hirfolyam.Presentation.Middlewares;

namespace Hirfolyam.Presentation.Extensions;

public static class WebApplicationExtension
{
    public static WebApplication UsePresentation(this WebApplication app)
    {
        app.UseMiddleware<ExceptionMiddleware>();
        app.MapControllers();
        app.MapOpenApi();
        app.UseSwaggerUI(options =>
        {
            options.RoutePrefix = "swagger";
            options.DocumentTitle = "Hirfolyam API";
            options.SwaggerEndpoint("../openapi/v1.json", "Hirfolyam API v1");
        });

        return app;
    }
}
