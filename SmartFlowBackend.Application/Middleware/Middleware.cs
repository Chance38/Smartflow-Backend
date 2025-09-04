using SmartFlowBackend.Domain.Contracts;

namespace Middleware;

public class ServiceMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ServiceMiddleware> _logger;

    public ServiceMiddleware(RequestDelegate next, ILogger<ServiceMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected exception.");

            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new ServerErrorSituation
            {
                RequestId = context.TraceIdentifier.ToString(),
                ErrorMessage = "Unexpected error, Please contact support with the request ID."
            }, new System.Text.Json.JsonSerializerOptions
            {
                PropertyNamingPolicy = null
            });
        }
    }

    public static string GetRequestId(HttpContext context)
    {
        return context.TraceIdentifier.ToString();
    }
}