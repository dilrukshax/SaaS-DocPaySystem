using UserService.Domain.ValueObjects;

namespace UserService.Domain.Entities;

public class User
{
    public Guid Id { get; private set; }
    public string Email { get; private set; }
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string? PhoneNumber { get; private set; }
    public string? Department { get; private set; }
    public string? JobTitle { get; private set; }
    public bool IsActive { get; private set; }
    public bool EmailConfirmed { get; private set; }
    public DateTime? LastLoginAt { get; private set; }
    public Guid TenantId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public string? ProfileImageUrl { get; private set; }
    public UserStatus Status { get; private set; }

    // Navigation properties
    public Tenant Tenant { get; private set; } = null!;

    private readonly List<UserRole> _userRoles = new();
    public IReadOnlyList<UserRole> UserRoles => _userRoles.AsReadOnly();

    private readonly List<UserSession> _sessions = new();
    public IReadOnlyList<UserSession> Sessions => _sessions.AsReadOnly();

    private User() { } // For EF Core

    public User(string email, string firstName, string lastName, Guid tenantId, 
        string? phoneNumber = null, string? department = null, string? jobTitle = null)
    {
        Id = Guid.NewGuid();
        Email = email?.ToLowerInvariant() ?? throw new ArgumentNullException(nameof(email));
        FirstName = firstName ?? throw new ArgumentNullException(nameof(firstName));
        LastName = lastName ?? throw new ArgumentNullException(nameof(lastName));
        PhoneNumber = phoneNumber;
        Department = department;
        JobTitle = jobTitle;
        TenantId = tenantId;
        IsActive = true;
        EmailConfirmed = false;
        Status = UserStatus.PendingActivation;
        CreatedAt = DateTime.UtcNow;
    }

    public string FullName => $"{FirstName} {LastName}";

    public void UpdateProfile(string firstName, string lastName, string? phoneNumber = null, 
        string? department = null, string? jobTitle = null)
    {
        FirstName = firstName ?? throw new ArgumentNullException(nameof(firstName));
        LastName = lastName ?? throw new ArgumentNullException(nameof(lastName));
        PhoneNumber = phoneNumber;
        Department = department;
        JobTitle = jobTitle;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ConfirmEmail()
    {
        EmailConfirmed = true;
        if (Status == UserStatus.PendingActivation)
        {
            Status = UserStatus.Active;
        }
        UpdatedAt = DateTime.UtcNow;
    }

    public void ChangeEmail(string newEmail)
    {
        Email = newEmail?.ToLowerInvariant() ?? throw new ArgumentNullException(nameof(newEmail));
        EmailConfirmed = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        Status = UserStatus.Active;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        Status = UserStatus.Inactive;
        UpdatedAt = DateTime.UtcNow;
        
        // End all active sessions
        foreach (var session in _sessions.Where(s => s.IsActive))
        {
            session.End();
        }
    }

    public void Suspend(string reason)
    {
        Status = UserStatus.Suspended;
        UpdatedAt = DateTime.UtcNow;
        
        // End all active sessions
        foreach (var session in _sessions.Where(s => s.IsActive))
        {
            session.End();
        }
    }

    public void AddRole(Guid roleId, Guid assignedBy)
    {
        if (_userRoles.Any(ur => ur.RoleId == roleId))
            return; // Role already assigned

        var userRole = new UserRole(Id, roleId, assignedBy);
        _userRoles.Add(userRole);
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveRole(Guid roleId)
    {
        var userRole = _userRoles.FirstOrDefault(ur => ur.RoleId == roleId);
        if (userRole != null)
        {
            _userRoles.Remove(userRole);
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public void RecordLogin(string? ipAddress = null, string? userAgent = null)
    {
        LastLoginAt = DateTime.UtcNow;
        
        var session = new UserSession(Id, ipAddress, userAgent);
        _sessions.Add(session);
        
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetProfileImage(string profileImageUrl)
    {
        ProfileImageUrl = profileImageUrl;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool HasRole(string roleName)
    {
        return _userRoles.Any(ur => ur.Role.Name.Equals(roleName, StringComparison.OrdinalIgnoreCase));
    }

    public bool HasPermission(string permissionName)
    {
        return _userRoles.Any(ur => ur.Role.RolePermissions.Any(rp => 
            rp.Permission.Name.Equals(permissionName, StringComparison.OrdinalIgnoreCase)));
    }
}
