namespace InvoiceService.Domain.Entities;

public class Vendor
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string? CompanyRegistrationNumber { get; private set; }
    public string? TaxIdentificationNumber { get; private set; }
    public string Email { get; private set; }
    public string? Phone { get; private set; }
    public string? Website { get; private set; }
    public Address Address { get; private set; }
    public bool IsActive { get; private set; }
    public Guid TenantId { get; private set; }
    public Guid CreatedBy { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private readonly List<Invoice> _invoices = new();
    public IReadOnlyList<Invoice> Invoices => _invoices.AsReadOnly();

    private Vendor() { } // For EF Core

    public Vendor(string name, string email, Address address, Guid tenantId, Guid createdBy,
        string? companyRegistrationNumber = null, string? taxIdentificationNumber = null,
        string? phone = null, string? website = null)
    {
        Id = Guid.NewGuid();
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Email = email ?? throw new ArgumentNullException(nameof(email));
        Address = address ?? throw new ArgumentNullException(nameof(address));
        CompanyRegistrationNumber = companyRegistrationNumber;
        TaxIdentificationNumber = taxIdentificationNumber;
        Phone = phone;
        Website = website;
        IsActive = true;
        TenantId = tenantId;
        CreatedBy = createdBy;
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdateContactInfo(string name, string email, string? phone = null, string? website = null)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Email = email ?? throw new ArgumentNullException(nameof(email));
        Phone = phone;
        Website = website;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateAddress(Address address)
    {
        Address = address ?? throw new ArgumentNullException(nameof(address));
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }
}

public record Address(
    string Street,
    string City,
    string State,
    string PostalCode,
    string Country
);
