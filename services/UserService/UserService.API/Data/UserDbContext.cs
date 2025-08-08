using Microsoft.EntityFrameworkCore;
using UserService.API.Models;

namespace UserService.API.Data
{
    public class UserDbContext : DbContext
    {
        public UserDbContext(DbContextOptions<UserDbContext> options) : base(options) { }
        
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<UserSubscription> UserSubscriptions { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Configure User entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(u => u.Email).IsUnique();
                entity.Property(u => u.Email).IsRequired();
                entity.Property(u => u.FirstName).IsRequired();
                entity.Property(u => u.LastName).IsRequired();
            });
            
            // Configure Role entity
            modelBuilder.Entity<Role>(entity =>
            {
                entity.HasIndex(r => r.Name).IsUnique();
                entity.Property(r => r.Name).IsRequired();
            });
            
            // Configure UserRole relationship
            modelBuilder.Entity<UserRole>(entity =>
            {
                entity.HasIndex(ur => new { ur.UserId, ur.RoleId }).IsUnique();
                
                entity.HasOne(ur => ur.User)
                    .WithMany(u => u.UserRoles)
                    .HasForeignKey(ur => ur.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
                    
                entity.HasOne(ur => ur.Role)
                    .WithMany(r => r.UserRoles)
                    .HasForeignKey(ur => ur.RoleId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
            
            // Configure UserSubscription relationship
            modelBuilder.Entity<UserSubscription>(entity =>
            {
                entity.HasOne(us => us.User)
                    .WithMany(u => u.UserSubscriptions)
                    .HasForeignKey(us => us.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
                    
                entity.Property(us => us.MonthlyFee)
                    .HasColumnType("decimal(18,2)");
            });
            
            // Seed default roles
            modelBuilder.Entity<Role>().HasData(
                new Role { Id = new Guid("10000000-0000-0000-0000-000000000001"), Name = "Admin", Description = "System Administrator", CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Role { Id = new Guid("10000000-0000-0000-0000-000000000002"), Name = "User", Description = "Regular User", CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Role { Id = new Guid("10000000-0000-0000-0000-000000000003"), Name = "Moderator", Description = "Content Moderator", CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
            );
        }
    }
}
