using Serilog;
using Serilog.Events;
using Swashbuckle.AspNetCore.Filters;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "SmartFlowBackend",
        Version = "v1",
        Description = "SmartFlowBackend API"
    });

    c.ExampleFilters();
});
builder.Services.AddSwaggerExamplesFromAssemblyOf<Program>();

var logger_url = Environment.GetEnvironmentVariable("LOGGER_URL") ?? "http://env.logger.url.is.not.given";
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .Enrich.WithProperty("ApplicationContext", "SmartFlowBackend")
    .Enrich.FromLogContext()
    .WriteTo.Console(LogEventLevel.Debug)
    .WriteTo.Seq(logger_url)
    .CreateLogger();
builder.Host.UseSerilog();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(UI =>
    {
        UI.SwaggerEndpoint("/swagger/v1/swagger.json", "SmartFlowBackend");
        UI.DocumentTitle = "SmartFlowBackend";
    });
}

app.MapControllers();
app.Run();

public partial class Program { }