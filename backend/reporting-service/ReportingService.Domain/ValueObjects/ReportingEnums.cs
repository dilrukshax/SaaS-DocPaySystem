namespace ReportingService.Domain.ValueObjects;

public enum ReportType
{
    Dashboard = 0,
    Financial = 1,
    Operational = 2,
    Compliance = 3,
    Analytics = 4,
    Custom = 5
}

public enum ReportFormat
{
    PDF = 0,
    Excel = 1,
    CSV = 2,
    JSON = 3,
    HTML = 4
}

public enum RequestStatus
{
    Pending = 0,
    Processing = 1,
    Completed = 2,
    Failed = 3,
    Cancelled = 4
}
