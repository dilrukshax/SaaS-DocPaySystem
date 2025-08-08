using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace Shared.Kernel.HealthChecks;

public static class HealthCheckExtensions
{
    public static IServiceCollection AddComprehensiveHealthChecks(
        this IServiceCollection services,
        string connectionString,
        string? serviceBusConnectionString = null,
        string? storageConnectionString = null)
    {
        var healthChecksBuilder = services.AddHealthChecks();

        // Database health check
        healthChecksBuilder.AddSqlServer(
            connectionString,
            name: "database",
            tags: new[] { "db", "sql", "ready" });

        // Service Bus health check (if configured)
        if (!string.IsNullOrEmpty(serviceBusConnectionString))
        {
            healthChecksBuilder.AddAzureServiceBusQueue(
                serviceBusConnectionString,
                queueName: "health-check",
                name: "servicebus",
                tags: new[] { "servicebus", "ready" });
        }

        // Azure Storage health check (if configured)
        if (!string.IsNullOrEmpty(storageConnectionString))
        {
            healthChecksBuilder.AddAzureBlobStorage(
                storageConnectionString,
                name: "storage",
                tags: new[] { "storage", "ready" });
        }

        // Memory health check
        healthChecksBuilder.AddCheck<MemoryHealthCheck>(
            "memory",
            tags: new[] { "memory" });

        return services;
    }

    public static IApplicationBuilder UseComprehensiveHealthChecks(this IApplicationBuilder app)
    {
        app.UseHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
        {
            ResponseWriter = WriteHealthCheckResponse
        });

        app.UseHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ready"),
            ResponseWriter = WriteHealthCheckResponse
        });

        app.UseHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
        {
            Predicate = _ => false,
            ResponseWriter = WriteHealthCheckResponse
        });

        return app;
    }

    private static async Task WriteHealthCheckResponse(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json; charset=utf-8";

        var response = new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(entry => new
            {
                name = entry.Key,
                status = entry.Value.Status.ToString(),
                exception = entry.Value.Exception?.Message,
                duration = entry.Value.Duration.ToString(),
                data = entry.Value.Data
            }),
            totalDuration = report.TotalDuration.ToString()
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }));
    }
}

public class MemoryHealthCheck : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var allocatedBytes = GC.GetTotalMemory(forceFullCollection: false);
        var data = new Dictionary<string, object>
        {
            ["AllocatedBytes"] = allocatedBytes,
            ["Gen0Collections"] = GC.CollectionCount(0),
            ["Gen1Collections"] = GC.CollectionCount(1),
            ["Gen2Collections"] = GC.CollectionCount(2)
        };

        var status = allocatedBytes < 1024L * 1024L * 1024L // 1 GB
            ? HealthStatus.Healthy
            : HealthStatus.Degraded;

        return Task.FromResult(new HealthCheckResult(
            status,
            description: "Reports degraded status if allocated bytes >= 1 GB.",
            data: data));
    }
}
