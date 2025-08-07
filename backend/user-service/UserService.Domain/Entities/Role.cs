namespace UserService.Domain.Entities;

public class Role
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public bool IsSystemRole { get; private set; }
    public Guid TenantId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    // Navigation properties
    public Tenant Tenant { get; private set; } = null!;

    private readonly List<UserRole> _userRoles = new();
    public IReadOnlyList<UserRole> UserRoles => _userRoles.AsReadOnly();

    private readonly List<RolePermission> _rolePermissions = new();
    public IReadOnlyList<RolePermission> RolePermissions => _rolePermissions.AsReadOnly();

    private Role() { } // For EF Core

    public Role(string name, string description, Guid tenantId, bool isSystemRole = false)
    {
        Id = Guid.NewGuid();
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description ?? string.Empty;
        TenantId = tenantId;
        IsSystemRole = isSystemRole;
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdateDetails(string name, string description)
    {
        if (IsSystemRole)
            throw new InvalidOperationException("Cannot update system roles");

        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description ?? string.Empty;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddPermission(Guid permissionId, Guid grantedBy)
    {
        if (_rolePermissions.Any(rp => rp.PermissionId == permissionId))
            return; // Permission already granted

        var rolePermission = new RolePermission(Id, permissionId, grantedBy);
        _rolePermissions.Add(rolePermission);
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemovePermission(Guid permissionId)
    {
        var rolePermission = _rolePermissions.FirstOrDefault(rp => rp.PermissionId == permissionId);
        if (rolePermission != null)
        {
            _rolePermissions.Remove(rolePermission);
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public bool HasPermission(string permissionName)
    {
        return _rolePermissions.Any(rp => 
            rp.Permission.Name.Equals(permissionName, StringComparison.OrdinalIgnoreCase));
    }
}
