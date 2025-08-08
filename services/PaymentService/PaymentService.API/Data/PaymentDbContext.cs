using Microsoft.EntityFrameworkCore;
using PaymentService.API.Models;

namespace PaymentService.API.Data
{
    public class PaymentDbContext : DbContext
    {
        public PaymentDbContext(DbContextOptions<PaymentDbContext> options) : base(options) { }
        
        public DbSet<Payment> Payments { get; set; }
        public DbSet<PaymentRefund> PaymentRefunds { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<InvoiceItem> InvoiceItems { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Configure Payment entity
            modelBuilder.Entity<Payment>(entity =>
            {
                entity.HasIndex(p => p.PaymentIntentId).IsUnique();
                entity.HasIndex(p => p.TransactionId);
                entity.HasIndex(p => new { p.UserId, p.CreatedAt });
                
                entity.Property(p => p.Amount).HasColumnType("decimal(18,2)");
                entity.Property(p => p.Currency).IsRequired().HasMaxLength(3);
                entity.Property(p => p.Status).IsRequired();
            });
            
            // Configure PaymentRefund relationship
            modelBuilder.Entity<PaymentRefund>(entity =>
            {
                entity.HasOne(pr => pr.Payment)
                    .WithMany(p => p.Refunds)
                    .HasForeignKey(pr => pr.PaymentId)
                    .OnDelete(DeleteBehavior.Cascade);
                    
                entity.Property(pr => pr.Amount).HasColumnType("decimal(18,2)");
            });
            
            // Configure Invoice entity
            modelBuilder.Entity<Invoice>(entity =>
            {
                entity.HasIndex(i => i.InvoiceNumber).IsUnique();
                entity.HasIndex(i => new { i.UserId, i.Status });
                
                entity.Property(i => i.Amount).HasColumnType("decimal(18,2)");
                entity.Property(i => i.TaxAmount).HasColumnType("decimal(18,2)");
                entity.Property(i => i.TotalAmount).HasColumnType("decimal(18,2)");
            });
            
            // Configure InvoiceItem relationship
            modelBuilder.Entity<InvoiceItem>(entity =>
            {
                entity.HasOne(ii => ii.Invoice)
                    .WithMany(i => i.InvoiceItems)
                    .HasForeignKey(ii => ii.InvoiceId)
                    .OnDelete(DeleteBehavior.Cascade);
                    
                entity.Property(ii => ii.UnitPrice).HasColumnType("decimal(18,2)");
                entity.Property(ii => ii.TotalPrice).HasColumnType("decimal(18,2)");
            });
        }
    }
}
