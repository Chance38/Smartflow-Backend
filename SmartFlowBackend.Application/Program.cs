using Serilog;
using Serilog.Events;
using Swashbuckle.AspNetCore.Filters;
using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;
using SmartFlowBackend.Infrastructure.Persistence;
using SmartFlowBackend.Domain.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var connectionString = builder.Configuration.GetConnectionString("Postgres_Connection");
builder.Services.AddDbContext<PostgresDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

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

app.UseCors("AllowAll");
app.MapControllers();
app.Run();

public partial class Program { }