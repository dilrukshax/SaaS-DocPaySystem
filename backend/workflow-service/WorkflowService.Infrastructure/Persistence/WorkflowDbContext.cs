using Microsoft.EntityFrameworkCore;
using WorkflowService.Domain.Entities;

namespace WorkflowService.Infrastructure.Persistence;

public class WorkflowDbContext : DbContext
{
    public WorkflowDbContext(DbContextOptions<WorkflowDbContext> options) : base(options)
    {
    }

    public DbSet<WorkflowDefinition> WorkflowDefinitions { get; set; }
    public DbSet<WorkflowInstance> WorkflowInstances { get; set; }
    public DbSet<WorkflowStep> WorkflowSteps { get; set; }
    public DbSet<WorkflowExecution> WorkflowExecutions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // WorkflowDefinition configuration
        modelBuilder.Entity<WorkflowDefinition>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Type).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Version).IsRequired();
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.TenantId).IsRequired();
            entity.Property(e => e.CreatedBy).IsRequired();
            entity.Property(e => e.UpdatedBy);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt);

            // Definition as JSON
            entity.Property(e => e.Definition)
                .HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                    v => System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new Dictionary<string, object>());

            // Indexes
            entity.HasIndex(e => e.TenantId);
            entity.HasIndex(e => e.Type);
            entity.HasIndex(e => new { e.TenantId, e.Type, e.IsActive });
            entity.HasIndex(e => new { e.TenantId, e.Name, e.Version }).IsUnique();

            // Relationships
            entity.HasMany(e => e.Steps)
                .WithOne()
                .HasForeignKey("WorkflowDefinitionId")
                .OnDelete(DeleteBehavior.Cascade);
        });

        // WorkflowInstance configuration
        modelBuilder.Entity<WorkflowInstance>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.WorkflowDefinitionId).IsRequired();
            entity.Property(e => e.Status).HasConversion<string>().IsRequired();
            entity.Property(e => e.TenantId).IsRequired();
            entity.Property(e => e.CreatedBy).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.StartedAt);
            entity.Property(e => e.CompletedAt);
            entity.Property(e => e.CurrentStepId);

            // Input/Output data as JSON
            entity.Property(e => e.InputData)
                .HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                    v => System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new Dictionary<string, object>());

            entity.Property(e => e.OutputData)
                .HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                    v => System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new Dictionary<string, object>());

            // Indexes
            entity.HasIndex(e => e.TenantId);
            entity.HasIndex(e => e.WorkflowDefinitionId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => new { e.TenantId, e.Status });

            // Relationships
            entity.HasMany(e => e.Executions)
                .WithOne()
                .HasForeignKey("WorkflowInstanceId")
                .OnDelete(DeleteBehavior.Cascade);
        });

        // WorkflowStep configuration
        modelBuilder.Entity<WorkflowStep>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Type).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Order).IsRequired();
            entity.Property(e => e.IsRequired).HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt);

            // Configuration as JSON
            entity.Property(e => e.Configuration)
                .HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                    v => System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new Dictionary<string, object>());

            // Conditions as JSON
            entity.Property(e => e.Conditions)
                .HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                    v => System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new Dictionary<string, object>());

            // Indexes
            entity.HasIndex("WorkflowDefinitionId");
            entity.HasIndex(e => e.Type);
            entity.HasIndex(e => e.Order);
        });

        // WorkflowExecution configuration
        modelBuilder.Entity<WorkflowExecution>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.StepId).IsRequired();
            entity.Property(e => e.StepName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Status).HasConversion<string>().IsRequired();
            entity.Property(e => e.AssignedTo);
            entity.Property(e => e.CompletedBy);
            entity.Property(e => e.ErrorMessage).HasMaxLength(1000);
            entity.Property(e => e.TenantId).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.StartedAt);
            entity.Property(e => e.CompletedAt);

            // Input/Output data as JSON
            entity.Property(e => e.InputData)
                .HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                    v => System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new Dictionary<string, object>());

            entity.Property(e => e.OutputData)
                .HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                    v => System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new Dictionary<string, object>());

            // Indexes
            entity.HasIndex(e => e.TenantId);
            entity.HasIndex(e => e.StepId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.AssignedTo);
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex("WorkflowInstanceId");
        });
    }
}
