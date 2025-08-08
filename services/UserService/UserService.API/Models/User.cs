using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserService.API.Models
{
    public class User
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        
        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        [StringLength(255)]
        public string PasswordHash { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;
        
        [StringLength(200)]
        public string? CompanyName { get; set; }
        
        [StringLength(20)]
        public string? PhoneNumber { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public bool EmailConfirmed { get; set; } = false;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
        
        public DateTime? LastLoginAt { get; set; }
        
        // Navigation properties
        public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
        public virtual ICollection<UserSubscription> UserSubscriptions { get; set; } = new List<UserSubscription>();
    }
    
    public class Role
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        
        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(255)]
        public string? Description { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
    
    public class UserRole
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        
        public Guid UserId { get; set; }
        public Guid RoleId { get; set; }
        
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
        
        [ForeignKey("RoleId")]
        public virtual Role Role { get; set; } = null!;
    }
    
    public class UserSubscription
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        
        public Guid UserId { get; set; }
        
        [Required]
        [StringLength(50)]
        public string SubscriptionType { get; set; } = string.Empty; // Basic, Pro, Enterprise
        
        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Active"; // Active, Suspended, Cancelled
        
        public DateTime StartDate { get; set; } = DateTime.UtcNow;
        public DateTime? EndDate { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal MonthlyFee { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        
        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
    }
}
