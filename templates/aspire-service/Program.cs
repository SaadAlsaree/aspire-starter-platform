using JasperFx;
using Microsoft.Extensions.Hosting;
using SharedInfrastructure.Persistence;
using Simble.Data;
using Simble.DependencyInjection;

var isCodeGeneration = args.Contains("codegen", StringComparer.OrdinalIgnoreCase);
var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddInfrastructure();
builder.AddApiServices(isCodeGeneration);

builder.Host
    .AddApplicationLogging()
    .AddApplicationMessaging(builder.Environment);

builder.Services.AddApplicationServices();

var app = builder.Build();

if (!isCodeGeneration)
{
    await app.Services.MigrateDatabaseAsync<AppDbContext>();
}

app.UseApplicationPipeline();

return await app.RunJasperFxCommands(args);
