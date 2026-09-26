using EnergyPlatform.Api.Configuration;
using EnergyPlatform.Application;
using EnergyPlatform.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration)
    .AddApi(builder.Configuration);

var app = builder.Build();

app.UseApiPipeline();
app.MapApiEndpoints();
await app.PrepareDatabaseAsync();

app.Run();

public partial class Program;
