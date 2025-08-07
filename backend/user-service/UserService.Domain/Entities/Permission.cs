namespace UserService.Domain.Entities;

public class Permission
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public string Category { get; private set; }
    public bool IsSystemPermission { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private readonly List<RolePermission> _rolePermissions = new();
    public IReadOnlyList<RolePermission> RolePermissions => _rolePermissions.AsReadOnly();

    private Permission() { } // For EF Core

    public Permission(string name, string description, string category, bool isSystemPermission = false)
    {
        Id = Guid.NewGuid();
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description ?? string.Empty;
        Category = category ?? throw new ArgumentNullException(nameof(category));
        IsSystemPermission = isSystemPermission;
        CreatedAt = DateTime.UtcNow;
    }
}

public class UserRole
{
    public Guid UserId { get; private set; }
    public Guid RoleId { get; private set; }
    public Guid AssignedBy { get; private set; }
    public DateTime AssignedAt { get; private set; }

    // Navigation properties
    public User User { get; private set; } = null!;
    public Role Role { get; private set; } = null!;

    private UserRole() { } // For EF Core

    public UserRole(Guid userId, Guid roleId, Guid assignedBy)
    {
        UserId = userId;
        RoleId = roleId;
        AssignedBy = assignedBy;
        AssignedAt = DateTime.UtcNow;
    }
}

public class RolePermission
{
    public Guid RoleId { get; private set; }
    public Guid PermissionId { get; private set; }
    public Guid GrantedBy { get; private set; }
    public DateTime GrantedAt { get; private set; }

    // Navigation properties
    public Role Role { get; private set; } = null!;
    public Permission Permission { get; private set; } = null!;

    private RolePermission() { } // For EF Core

    public RolePermission(Guid roleId, Guid permissionId, Guid grantedBy)
    {
        RoleId = roleId;
        PermissionId = permissionId;
        GrantedBy = grantedBy;
        GrantedAt = DateTime.UtcNow;
    }
}
