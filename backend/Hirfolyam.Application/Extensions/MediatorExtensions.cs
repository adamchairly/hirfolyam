using FluentValidation;
using Hirfolyam.Application.Behaviors;
using Hirfolyam.Domain.Entities;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Hirfolyam.Application.Extensions;

public static class MediatorExtensions
{
    public static IServiceCollection AddApplicationMediator(this IServiceCollection services)
    {
        var applicationAssembly = typeof(MediatorExtensions).Assembly;
        var domainAssembly = typeof(Article).Assembly;

        services.AddMediatR(configuration =>
        {
            configuration.RegisterServicesFromAssembly(applicationAssembly);
            configuration.RegisterServicesFromAssembly(domainAssembly);
        });

        services.AddValidatorsFromAssembly(applicationAssembly);
        services.AddValidatorsFromAssembly(domainAssembly);
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        return services;
    }
}
