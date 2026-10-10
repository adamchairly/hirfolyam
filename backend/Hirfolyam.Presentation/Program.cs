using Hirfolyam.Application.Extensions;
using Hirfolyam.Infrastructure.Extensions;
using Hirfolyam.Presentation.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApplication(builder.Configuration)
    .AddApplicationMediator()
    .AddInfrastructure(builder.Configuration)
    .AddPresentation(builder.Configuration);

await using var app = builder.Build();

app.UsePresentation();

await app.Services.InitializeInfrastructureAsync();
await app.RunAsync();
