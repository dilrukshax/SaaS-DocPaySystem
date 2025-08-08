using System.Diagnostics;
using System.Diagnostics.Metrics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Shared.Kernel.Monitoring;

public interface IMetricsService
{
    void IncrementCounter(string name, string[]? tags = null);
    void RecordValue(string name, double value, string[]? tags = null);
    void RecordDuration(string name, TimeSpan duration, string[]? tags = null);
    IDisposable StartTimer(string name, string[]? tags = null);
}

public class MetricsService : IMetricsService
{
    private readonly Meter _meter;
    private readonly ILogger<MetricsService> _logger;
    private readonly Dictionary<string, Counter<long>> _counters = new();
    private readonly Dictionary<string, Histogram<double>> _histograms = new();

    public MetricsService(ILogger<MetricsService> logger)
    {
        _logger = logger;
        _meter = new Meter("SaaS.DocPaySystem", "1.0.0");
    }

    public void IncrementCounter(string name, string[]? tags = null)
    {
        try
        {
            if (!_counters.TryGetValue(name, out var counter))
            {
                counter = _meter.CreateCounter<long>(name);
                _counters[name] = counter;
            }

            var tagPairs = ConvertToTagList(tags);
            counter.Add(1, tagPairs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to increment counter {CounterName}", name);
        }
    }

    public void RecordValue(string name, double value, string[]? tags = null)
    {
        try
        {
            if (!_histograms.TryGetValue(name, out var histogram))
            {
                histogram = _meter.CreateHistogram<double>(name);
                _histograms[name] = histogram;
            }

            var tagPairs = ConvertToTagList(tags);
            histogram.Record(value, tagPairs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to record value for {MetricName}", name);
        }
    }

    public void RecordDuration(string name, TimeSpan duration, string[]? tags = null)
    {
        RecordValue(name, duration.TotalMilliseconds, tags);
    }

    public IDisposable StartTimer(string name, string[]? tags = null)
    {
        return new TimerScope(this, name, tags);
    }

    private static TagList ConvertToTagList(string[]? tags)
    {
        var tagList = new TagList();
        if (tags != null)
        {
            for (int i = 0; i < tags.Length - 1; i += 2)
            {
                tagList.Add(tags[i], tags[i + 1]);
            }
        }
        return tagList;
    }

    private class TimerScope : IDisposable
    {
        private readonly MetricsService _metricsService;
        private readonly string _name;
        private readonly string[]? _tags;
        private readonly Stopwatch _stopwatch;

        public TimerScope(MetricsService metricsService, string name, string[]? tags)
        {
            _metricsService = metricsService;
            _name = name;
            _tags = tags;
            _stopwatch = Stopwatch.StartNew();
        }

        public void Dispose()
        {
            _stopwatch.Stop();
            _metricsService.RecordDuration(_name, _stopwatch.Elapsed, _tags);
        }
    }
}

public static class MetricsExtensions
{
    public static IServiceCollection AddMetrics(this IServiceCollection services)
    {
        services.AddSingleton<IMetricsService, MetricsService>();
        return services;
    }
}

// Common metric names
public static class MetricNames
{
    // HTTP Metrics
    public const string HttpRequestDuration = "http_request_duration_ms";
    public const string HttpRequestCount = "http_requests_total";
    public const string HttpRequestSize = "http_request_size_bytes";
    public const string HttpResponseSize = "http_response_size_bytes";

    // Database Metrics
    public const string DatabaseQueryDuration = "database_query_duration_ms";
    public const string DatabaseConnectionCount = "database_connections_total";
    public const string DatabaseErrorCount = "database_errors_total";

    // Business Metrics
    public const string DocumentsProcessed = "documents_processed_total";
    public const string InvoicesGenerated = "invoices_generated_total";
    public const string PaymentsProcessed = "payments_processed_total";
    public const string WorkflowsExecuted = "workflows_executed_total";
    public const string NotificationsSent = "notifications_sent_total";

    // Error Metrics
    public const string ErrorCount = "errors_total";
    public const string ExceptionCount = "exceptions_total";

    // Performance Metrics
    public const string MemoryUsage = "memory_usage_bytes";
    public const string CpuUsage = "cpu_usage_percent";
    public const string QueueLength = "queue_length";
}
