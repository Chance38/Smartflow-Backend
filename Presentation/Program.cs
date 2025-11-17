using Serilog;
using Serilog.Events;
using Swashbuckle.AspNetCore.Filters;
using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Persistence;
using Application.SwaggerSetting;
using Domain.Interface;
using Application.Service;
using Application.Interface.Service;
using Infrastructure.Persistence.Repository;
using Infrastructure.Messaging;
using Domain.Subscriber;
using Middleware;

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

builder.Services.AddSingleton<RabbitMqConnection>();
builder.Services.AddSingleton<IIdentityHostService, IdentityHostService>();

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IRecordRepository, RecordRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ITagRepository, TagRepository>();
builder.Services.AddScoped<IBalanceRepository, BalanceRepository>();
builder.Services.AddScoped<ISummaryRepository, SummaryRepository>();
builder.Services.AddScoped<IRecordTemplateRepository, RecordTemplateRepository>();

builder.Services.AddSingleton<UserRegisterSubscriber>();
builder.Services.AddSingleton<IUserRegisterSubscriber>(sp => sp.GetRequiredService<UserRegisterSubscriber>());
builder.Services.AddHostedService<UserRegisterSubscriber>();

builder.Services.AddScoped<IRecordService, RecordService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ITagService, TagService>();
builder.Services.AddScoped<IBalanceService, BalanceService>();
builder.Services.AddScoped<ISummaryService, SummaryService>();
builder.Services.AddScoped<IRecordTemplateService, RecordTemplateService>();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter(System.Text.Json.JsonNamingPolicy.CamelCase));
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
    c.DocumentFilter<ServerDocumentFilter>();

    c.SupportNonNullableReferenceTypes();
    c.ExampleFilters();

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: 'Bearer {token}'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
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

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var dbContext = services.GetRequiredService<PostgresDbContext>();
        dbContext.Database.Migrate();
        var uow = services.GetRequiredService<IUnitOfWork>();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating or seeding the database.");
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(UI =>
    {
        UI.SwaggerEndpoint("/swagger/v1/swagger.json", "SmartFlowBackend");
        UI.DocumentTitle = "SmartFlowBackend";
        UI.ConfigObject.AdditionalItems["persistAuthorization"] = true;
    });
}

app.UseMiddleware<ServiceMiddleware>();

app.UseCors("AllowAll");
app.MapControllers();
app.Run();

public partial class Program { }