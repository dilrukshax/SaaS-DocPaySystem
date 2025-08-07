using Microsoft.EntityFrameworkCore;
using DocumentService.Domain.Entities;

namespace DocumentService.Infrastructure.Persistence;

public class DocumentDbContext : DbContext
{
    public DocumentDbContext(DbContextOptions<DocumentDbContext> options) : base(options)
    {
    }

    public DbSet<Document> Documents { get; set; }
    public DbSet<DocumentVersion> DocumentVersions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Document configuration
        modelBuilder.Entity<Document>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(255).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.FileName).HasMaxLength(255).IsRequired();
            entity.Property(e => e.MimeType).HasMaxLength(100).IsRequired();
            entity.Property(e => e.StoragePath).HasMaxLength(500).IsRequired();
            entity.Property(e => e.Status).HasConversion<string>();
            entity.Property(e => e.TenantId).IsRequired();
            entity.Property(e => e.CreatedBy).IsRequired();
            entity.Property(e => e.UpdatedBy);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt);
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.DeletedAt);
            entity.Property(e => e.DeletedBy);

            // OCR data
            entity.Property(e => e.ExtractedText);
            entity.Property(e => e.OCRConfidence);

            // Convert Tags collection to JSON
            entity.Property(e => e.Tags)
                .HasConversion(
                    v => string.Join(',', v),
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList());

            // Indexes
            entity.HasIndex(e => e.TenantId);
            entity.HasIndex(e => e.CreatedBy);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => new { e.TenantId, e.IsDeleted });

            // Relationships
            entity.HasMany(e => e.Versions)
                .WithOne()
                .HasForeignKey("DocumentId")
                .OnDelete(DeleteBehavior.Cascade);
        });

        // DocumentVersion configuration
        modelBuilder.Entity<DocumentVersion>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.VersionNumber).IsRequired();
            entity.Property(e => e.FileName).HasMaxLength(255).IsRequired();
            entity.Property(e => e.MimeType).HasMaxLength(100).IsRequired();
            entity.Property(e => e.StoragePath).HasMaxLength(500).IsRequired();
            entity.Property(e => e.Comment).HasMaxLength(500);
            entity.Property(e => e.CreatedBy).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();

            // Indexes
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex("DocumentId");
        });
    }
}
