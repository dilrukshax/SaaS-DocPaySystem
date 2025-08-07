namespace DocumentService.Domain.Entities;

public class Metadata
{
    public Guid Id { get; private set; }
    public Guid DocumentId { get; private set; }
    public string Key { get; private set; }
    public string Value { get; private set; }
    public Guid CreatedBy { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    // Navigation property
    public Document Document { get; private set; } = null!;

    private Metadata() { } // For EF Core

    public Metadata(Guid documentId, string key, string value, Guid createdBy)
    {
        Id = Guid.NewGuid();
        DocumentId = documentId;
        Key = key ?? throw new ArgumentNullException(nameof(key));
        Value = value ?? throw new ArgumentNullException(nameof(value));
        CreatedBy = createdBy;
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdateValue(string value)
    {
        Value = value ?? throw new ArgumentNullException(nameof(value));
        UpdatedAt = DateTime.UtcNow;
    }
}
