using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Application.Contract;

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
        if (context.Request.Headers.TryGetValue("requestId", out var requestId) && !string.IsNullOrWhiteSpace(requestId))
        {
            context.TraceIdentifier = requestId.ToString();
        }
        context.Items["RequestId"] = context.TraceIdentifier;

        if (!context.Request.Headers.TryGetValue("Authorization", out var authHeader) || string.IsNullOrWhiteSpace(authHeader))
        {
            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new ClientErrorSituation
            {
                RequestId = context.TraceIdentifier,
                ErrorMessage = "Missing Authorization header"
            }, new System.Text.Json.JsonSerializerOptions
            {
                PropertyNamingPolicy = null
            });
            return;
        }

        var token = authHeader.ToString().Replace("Bearer ", "", StringComparison.OrdinalIgnoreCase).Trim();
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes("SmartechAFk9Jlh9qTPXWLJxGjsoglsigaoGJIKey");

        try
        {
            var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = "SmartechIssuer",
                ValidateAudience = true,
                ValidAudience = "SmartechAudience",
                ValidateLifetime = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuerSigningKey = true,
                ClockSkew = TimeSpan.Zero
            }, out var validatedToken);

            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new ClientErrorSituation
                {
                    RequestId = context.TraceIdentifier,
                    ErrorMessage = "Token does not contain a valid userId"
                }, new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNamingPolicy = null
                });
                return;
            }
            context.Items["UserId"] = userId;

            await _next(context);
        }
        catch (SecurityTokenExpiredException)
        {
            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new ClientErrorSituation
            {
                RequestId = context.TraceIdentifier,
                ErrorMessage = "Token expired"
            }, new System.Text.Json.JsonSerializerOptions
            {
                PropertyNamingPolicy = null
            });
        }
        catch (SecurityTokenInvalidIssuerException)
        {
            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new ClientErrorSituation
            {
                RequestId = context.TraceIdentifier,
                ErrorMessage = "Invalid issuer"
            }, new System.Text.Json.JsonSerializerOptions
            {
                PropertyNamingPolicy = null
            });
        }
        catch (SecurityTokenInvalidAudienceException)
        {
            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new ClientErrorSituation
            {
                RequestId = context.TraceIdentifier,
                ErrorMessage = "Invalid audience"
            }, new System.Text.Json.JsonSerializerOptions
            {
                PropertyNamingPolicy = null
            });
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
        return context.TraceIdentifier;
    }

    public static Guid GetUserId(HttpContext context)
    {
        if (context.Items.TryGetValue("UserId", out var userIdObj) && userIdObj is Guid userId)
        {
            return userId;
        }

        throw new InvalidOperationException("UserId not found in HttpContext.");
    }
}