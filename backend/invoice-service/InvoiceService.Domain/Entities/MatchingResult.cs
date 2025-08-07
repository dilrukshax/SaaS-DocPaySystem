namespace InvoiceService.Domain.Entities;

public class MatchingResult
{
    public Guid Id { get; private set; }
    public Guid InvoiceId { get; private set; }
    public bool IsMatched { get; private set; }
    public string Details { get; private set; }
    public Guid PerformedBy { get; private set; }
    public DateTime PerformedAt { get; private set; }
    public string? PurchaseOrderReference { get; private set; }
    public string? ReceiptReference { get; private set; }
    public decimal? MatchingScore { get; private set; }

    // Navigation property
    public Invoice Invoice { get; private set; } = null!;

    private MatchingResult() { } // For EF Core

    public MatchingResult(Guid invoiceId, bool isMatched, string details, Guid performedBy)
    {
        Id = Guid.NewGuid();
        InvoiceId = invoiceId;
        IsMatched = isMatched;
        Details = details ?? throw new ArgumentNullException(nameof(details));
        PerformedBy = performedBy;
        PerformedAt = DateTime.UtcNow;
    }

    public void SetReferences(string? purchaseOrderReference, string? receiptReference)
    {
        PurchaseOrderReference = purchaseOrderReference;
        ReceiptReference = receiptReference;
    }

    public void SetMatchingScore(decimal score)
    {
        MatchingScore = Math.Max(0, Math.Min(100, score)); // Ensure score is between 0-100
    }
}
