using System.Diagnostics;

namespace KodaStore.Api.Middlewares;

public sealed class RequestTiming(RequestDelegate next, ILogger<RequestTiming> logger)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<RequestTiming> _logger = logger;

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();

            _logger.LogInformation("Http {Method} {Path} -> {StatusCode} in {ElapsedMs} ms",
            context.Request.Method,
            context.Request.Path,
            context.Response.StatusCode,
            stopwatch.ElapsedMilliseconds);
        }
    }
}