using Hirfolyam.Application.Shared;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace Hirfolyam.Presentation.Extensions;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddPresentation(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers().ConfigureApiBehaviorOptions(options =>
        {
            options.InvalidModelStateResponseFactory = context => new BadRequestObjectResult(new BaseResponse
            {
                Error = ErrorCodes.BadRequest,
                Message = string.Join("; ", context.ModelState.Values
                    .SelectMany(value => value.Errors)
                    .Select(error => error.ErrorMessage)),
                Code = 400
            });
        });

        services.AddOpenApi();

        services.AddSerilog((provider, logging) => logging
            .ReadFrom.Configuration(configuration)
            .ReadFrom.Services(provider)
            .Enrich.FromLogContext()
            .WriteToApplicationConsole());

        services.AddJobs(configuration);

        return services;
    }
}
