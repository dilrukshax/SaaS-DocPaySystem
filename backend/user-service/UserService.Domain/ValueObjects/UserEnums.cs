namespace UserService.Domain.ValueObjects;

public enum UserStatus
{
    PendingActivation = 0,
    Active = 1,
    Inactive = 2,
    Suspended = 3,
    Locked = 4
}

public enum TenantStatus
{
    Active = 0,
    Suspended = 1,
    Expired = 2,
    Trial = 3
}

public record TenantSettings(
    bool AllowSelfRegistration = false,
    bool RequireEmailConfirmation = true,
    int SessionTimeoutMinutes = 480,
    bool EnableTwoFactorAuth = false,
    string TimeZone = "UTC",
    string DateFormat = "yyyy-MM-dd",
    bool EnableAuditLog = true
);
