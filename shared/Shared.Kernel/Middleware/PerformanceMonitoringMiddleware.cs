using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Shared.Kernel.Monitoring;
using System.Diagnostics;

namespace Shared.Kernel.Middleware;

public class PerformanceMonitoringMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<PerformanceMonitoringMiddleware> _logger;
    private readonly IMetricsService _metricsService;

    public PerformanceMonitoringMiddleware(
        RequestDelegate next,
        ILogger<PerformanceMonitoringMiddleware> logger,
        IMetricsService metricsService)
    {
        _next = next;
        _logger = logger;
        _metricsService = metricsService;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var requestSize = context.Request.ContentLength ?? 0;

        // Increment request counter
        _metricsService.IncrementCounter(MetricNames.HttpRequestCount, new[]
        {
            "method", context.Request.Method,
            "endpoint", context.Request.Path.Value ?? "unknown"
        });

        // Record request size
        _metricsService.RecordValue(MetricNames.HttpRequestSize, requestSize, new[]
        {
            "method", context.Request.Method,
            "endpoint", context.Request.Path.Value ?? "unknown"
        });

        Exception? exception = null;
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            exception = ex;
            throw;
        }
        finally
        {
            stopwatch.Stop();

            // Record response time
            _metricsService.RecordDuration(MetricNames.HttpRequestDuration, stopwatch.Elapsed, new[]
            {
                "method", context.Request.Method,
                "endpoint", context.Request.Path.Value ?? "unknown",
                "status_code", context.Response.StatusCode.ToString()
            });

            // Record response size if available
            if (context.Response.ContentLength.HasValue)
            {
                _metricsService.RecordValue(MetricNames.HttpResponseSize, context.Response.ContentLength.Value, new[]
                {
                    "method", context.Request.Method,
                    "endpoint", context.Request.Path.Value ?? "unknown",
                    "status_code", context.Response.StatusCode.ToString()
                });
            }

            // Log slow requests
            if (stopwatch.ElapsedMilliseconds > 1000) // Slow request threshold: 1 second
            {
                _logger.LogWarning(
                    "Slow request detected: {Method} {Path} took {ElapsedMs}ms, Status: {StatusCode}",
                    context.Request.Method,
                    context.Request.Path,
                    stopwatch.ElapsedMilliseconds,
                    context.Response.StatusCode);
            }

            // Log errors
            if (exception != null)
            {
                _metricsService.IncrementCounter(MetricNames.ExceptionCount, new[]
                {
                    "method", context.Request.Method,
                    "endpoint", context.Request.Path.Value ?? "unknown",
                    "exception_type", exception.GetType().Name
                });

                _logger.LogError(exception,
                    "Request failed: {Method} {Path} in {ElapsedMs}ms",
                    context.Request.Method,
                    context.Request.Path,
                    stopwatch.ElapsedMilliseconds);
            }
            else if (context.Response.StatusCode >= 400)
            {
                _metricsService.IncrementCounter(MetricNames.ErrorCount, new[]
                {
                    "method", context.Request.Method,
                    "endpoint", context.Request.Path.Value ?? "unknown",
                    "status_code", context.Response.StatusCode.ToString()
                });

                if (context.Response.StatusCode >= 500)
                {
                    _logger.LogError(
                        "Server error: {Method} {Path} returned {StatusCode} in {ElapsedMs}ms",
                        context.Request.Method,
                        context.Request.Path,
                        context.Response.StatusCode,
                        stopwatch.ElapsedMilliseconds);
                }
            }
        }
    }
}
