using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PaymentService.API.Models
{
    public class Payment
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        
        public Guid UserId { get; set; }
        public Guid? InvoiceId { get; set; }
        
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }
        
        [Required]
        [StringLength(3)]
        public string Currency { get; set; } = "USD";
        
        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Pending"; // Pending, Processing, Completed, Failed, Refunded
        
        [StringLength(100)]
        public string? PaymentMethod { get; set; } // Card, Bank Transfer, etc.
        
        [StringLength(255)]
        public string? PaymentIntentId { get; set; } // Stripe Payment Intent ID
        
        [StringLength(255)]
        public string? TransactionId { get; set; } // External transaction reference
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        [StringLength(1000)]
        public string? Metadata { get; set; } // JSON metadata
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ProcessedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        
        // Navigation properties
        public virtual ICollection<PaymentRefund> Refunds { get; set; } = new List<PaymentRefund>();
    }
    
    public class PaymentRefund
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        
        public Guid PaymentId { get; set; }
        
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Pending"; // Pending, Completed, Failed
        
        [StringLength(255)]
        public string? RefundId { get; set; } // External refund reference
        
        [StringLength(500)]
        public string? Reason { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ProcessedAt { get; set; }
        
        // Navigation properties
        [ForeignKey("PaymentId")]
        public virtual Payment Payment { get; set; } = null!;
    }
    
    public class Invoice
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        
        public Guid UserId { get; set; }
        
        [Required]
        [StringLength(50)]
        public string InvoiceNumber { get; set; } = string.Empty;
        
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }
        
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TaxAmount { get; set; } = 0;
        
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }
        
        [Required]
        [StringLength(3)]
        public string Currency { get; set; } = "USD";
        
        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Draft"; // Draft, Sent, Paid, Overdue, Cancelled
        
        public DateTime IssueDate { get; set; } = DateTime.UtcNow;
        public DateTime DueDate { get; set; }
        
        [StringLength(1000)]
        public string? Description { get; set; }
        
        [StringLength(2000)]
        public string? Notes { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        
        // Navigation properties
        public virtual ICollection<InvoiceItem> InvoiceItems { get; set; } = new List<InvoiceItem>();
    }
    
    public class InvoiceItem
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        
        public Guid InvoiceId { get; set; }
        
        [Required]
        [StringLength(255)]
        public string Description { get; set; } = string.Empty;
        
        [Required]
        public int Quantity { get; set; } = 1;
        
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }
        
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }
        
        // Navigation properties
        [ForeignKey("InvoiceId")]
        public virtual Invoice Invoice { get; set; } = null!;
    }
}
