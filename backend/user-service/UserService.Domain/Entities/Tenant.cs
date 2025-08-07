using UserService.Domain.ValueObjects;

namespace UserService.Domain.Entities;

public class Tenant
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string? Domain { get; private set; }
    public TenantStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public TenantSettings Settings { get; private set; }
    public int MaxUsers { get; private set; }
    public DateTime? SubscriptionExpiresAt { get; private set; }

    private readonly List<User> _users = new();
    public IReadOnlyList<User> Users => _users.AsReadOnly();

    private readonly List<Role> _roles = new();
    public IReadOnlyList<Role> Roles => _roles.AsReadOnly();

    private Tenant() { } // For EF Core

    public Tenant(string name, string? domain = null, int maxUsers = 10)
    {
        Id = Guid.NewGuid();
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Domain = domain;
        Status = TenantStatus.Active;
        MaxUsers = maxUsers;
        Settings = new TenantSettings();
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdateName(string name)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateDomain(string? domain)
    {
        Domain = domain;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateSettings(TenantSettings settings)
    {
        Settings = settings ?? throw new ArgumentNullException(nameof(settings));
        UpdatedAt = DateTime.UtcNow;
    }

    public void Suspend()
    {
        Status = TenantStatus.Suspended;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        Status = TenantStatus.Active;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetSubscription(DateTime expiresAt, int maxUsers)
    {
        SubscriptionExpiresAt = expiresAt;
        MaxUsers = maxUsers;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool CanAddUser()
    {
        return _users.Count(u => u.IsActive) < MaxUsers;
    }
}

public class UserSession
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public DateTime StartedAt { get; private set; }
    public DateTime? EndedAt { get; private set; }
    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }
    public bool IsActive => EndedAt == null;

    // Navigation property
    public User User { get; private set; } = null!;

    private UserSession() { } // For EF Core

    public UserSession(Guid userId, string? ipAddress = null, string? userAgent = null)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        StartedAt = DateTime.UtcNow;
        IpAddress = ipAddress;
        UserAgent = userAgent;
    }

    public void End()
    {
        EndedAt = DateTime.UtcNow;
    }
}
