namespace InvoiceService.Domain.Entities;

public class LineItem
{
    public Guid Id { get; private set; }
    public Guid InvoiceId { get; private set; }
    public string Description { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal LineTotal { get; private set; }
    public string? ProductCode { get; private set; }
    public decimal TaxRate { get; private set; }
    public decimal TaxAmount { get; private set; }
    public int LineNumber { get; private set; }

    // Navigation property
    public Invoice Invoice { get; private set; } = null!;

    private LineItem() { } // For EF Core

    public LineItem(Guid invoiceId, string description, decimal quantity, decimal unitPrice, 
        string? productCode = null, decimal taxRate = 0)
    {
        Id = Guid.NewGuid();
        InvoiceId = invoiceId;
        Description = description ?? throw new ArgumentNullException(nameof(description));
        Quantity = quantity;
        UnitPrice = unitPrice;
        ProductCode = productCode;
        TaxRate = taxRate;
        
        CalculateTotals();
    }

    public void UpdateQuantity(decimal quantity)
    {
        Quantity = quantity;
        CalculateTotals();
    }

    public void UpdateUnitPrice(decimal unitPrice)
    {
        UnitPrice = unitPrice;
        CalculateTotals();
    }

    public void UpdateTaxRate(decimal taxRate)
    {
        TaxRate = taxRate;
        CalculateTotals();
    }

    public void SetLineNumber(int lineNumber)
    {
        LineNumber = lineNumber;
    }

    private void CalculateTotals()
    {
        LineTotal = Quantity * UnitPrice;
        TaxAmount = LineTotal * (TaxRate / 100);
    }
}
