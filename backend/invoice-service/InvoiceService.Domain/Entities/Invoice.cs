using InvoiceService.Domain.ValueObjects;

namespace InvoiceService.Domain.Entities;

public class Invoice
{
    public Guid Id { get; private set; }
    public string InvoiceNumber { get; private set; }
    public Guid VendorId { get; private set; }
    public string? PurchaseOrderNumber { get; private set; }
    public DateTime InvoiceDate { get; private set; }
    public DateTime DueDate { get; private set; }
    public decimal SubTotal { get; private set; }
    public decimal TaxAmount { get; private set; }
    public decimal TotalAmount { get; private set; }
    public string Currency { get; private set; }
    public InvoiceStatus Status { get; private set; }
    public Guid TenantId { get; private set; }
    public Guid CreatedBy { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public string? Notes { get; private set; }
    public bool IsDeleted { get; private set; }

    // Navigation properties
    public Vendor Vendor { get; private set; } = null!;

    private readonly List<LineItem> _lineItems = new();
    public IReadOnlyList<LineItem> LineItems => _lineItems.AsReadOnly();

    private readonly List<MatchingResult> _matchingResults = new();
    public IReadOnlyList<MatchingResult> MatchingResults => _matchingResults.AsReadOnly();

    private Invoice() { } // For EF Core

    public Invoice(string invoiceNumber, Guid vendorId, DateTime invoiceDate, DateTime dueDate,
        string currency, Guid tenantId, Guid createdBy, string? purchaseOrderNumber = null, string? notes = null)
    {
        Id = Guid.NewGuid();
        InvoiceNumber = invoiceNumber ?? throw new ArgumentNullException(nameof(invoiceNumber));
        VendorId = vendorId;
        PurchaseOrderNumber = purchaseOrderNumber;
        InvoiceDate = invoiceDate;
        DueDate = dueDate;
        Currency = currency ?? throw new ArgumentNullException(nameof(currency));
        Status = InvoiceStatus.Draft;
        TenantId = tenantId;
        CreatedBy = createdBy;
        CreatedAt = DateTime.UtcNow;
        Notes = notes;
        IsDeleted = false;
        SubTotal = 0;
        TaxAmount = 0;
        TotalAmount = 0;
    }

    public void AddLineItem(string description, decimal quantity, decimal unitPrice, 
        string? productCode = null, decimal taxRate = 0)
    {
        var lineItem = new LineItem(Id, description, quantity, unitPrice, productCode, taxRate);
        _lineItems.Add(lineItem);
        
        RecalculateTotals();
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveLineItem(Guid lineItemId)
    {
        var lineItem = _lineItems.FirstOrDefault(li => li.Id == lineItemId);
        if (lineItem != null)
        {
            _lineItems.Remove(lineItem);
            RecalculateTotals();
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public void UpdateStatus(InvoiceStatus status, Guid updatedBy, string? reason = null)
    {
        Status = status;
        UpdatedAt = DateTime.UtcNow;
        
        // Add matching result if status change is approval related
        if (status == InvoiceStatus.Approved || status == InvoiceStatus.Rejected)
        {
            var matchingResult = new MatchingResult(Id, status == InvoiceStatus.Approved, 
                reason ?? "Manual approval", updatedBy);
            _matchingResults.Add(matchingResult);
        }
    }

    public void AddMatchingResult(bool isMatched, string details, Guid performedBy)
    {
        var matchingResult = new MatchingResult(Id, isMatched, details, performedBy);
        _matchingResults.Add(matchingResult);
        
        // Auto-approve if three-way matching is successful
        if (isMatched && Status == InvoiceStatus.PendingApproval)
        {
            Status = InvoiceStatus.Approved;
        }
        
        UpdatedAt = DateTime.UtcNow;
    }

    public void SubmitForApproval()
    {
        if (Status != InvoiceStatus.Draft)
            throw new InvalidOperationException("Only draft invoices can be submitted for approval");
            
        if (!_lineItems.Any())
            throw new InvalidOperationException("Cannot submit invoice without line items");

        Status = InvoiceStatus.PendingApproval;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SoftDelete(Guid deletedBy)
    {
        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
    }

    private void RecalculateTotals()
    {
        SubTotal = _lineItems.Sum(li => li.LineTotal);
        TaxAmount = _lineItems.Sum(li => li.TaxAmount);
        TotalAmount = SubTotal + TaxAmount;
    }
}
