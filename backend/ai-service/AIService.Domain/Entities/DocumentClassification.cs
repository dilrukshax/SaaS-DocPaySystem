using AIService.Domain.ValueObjects;

namespace AIService.Domain.Entities;

public class DocumentClassification
{
    public Guid Id { get; private set; }
    public Guid DocumentId { get; private set; }
    public string DocumentType { get; private set; }
    public decimal ConfidenceScore { get; private set; }
    public string ExtractedText { get; private set; }
    public string? KeyValuePairs { get; private set; }
    public ClassificationStatus Status { get; private set; }
    public string ModelVersion { get; private set; }
    public DateTime ProcessedAt { get; private set; }
    public Guid TenantId { get; private set; }
    public string? ErrorMessage { get; private set; }

    private readonly List<EntityExtraction> _entities = new();
    public IReadOnlyList<EntityExtraction> Entities => _entities.AsReadOnly();

    private DocumentClassification() { } // For EF Core

    public DocumentClassification(Guid documentId, string documentType, decimal confidenceScore, 
        string extractedText, string modelVersion, Guid tenantId)
    {
        Id = Guid.NewGuid();
        DocumentId = documentId;
        DocumentType = documentType ?? throw new ArgumentNullException(nameof(documentType));
        ConfidenceScore = Math.Max(0, Math.Min(100, confidenceScore));
        ExtractedText = extractedText ?? throw new ArgumentNullException(nameof(extractedText));
        ModelVersion = modelVersion ?? throw new ArgumentNullException(nameof(modelVersion));
        TenantId = tenantId;
        Status = ClassificationStatus.Completed;
        ProcessedAt = DateTime.UtcNow;
    }

    public void AddEntity(string entityType, string value, decimal confidence, int startPosition, int endPosition)
    {
        var entity = new EntityExtraction(Id, entityType, value, confidence, startPosition, endPosition);
        _entities.Add(entity);
    }

    public void SetKeyValuePairs(string keyValuePairs)
    {
        KeyValuePairs = keyValuePairs;
    }

    public void MarkAsFailed(string errorMessage)
    {
        Status = ClassificationStatus.Failed;
        ErrorMessage = errorMessage;
    }
}

public class EntityExtraction
{
    public Guid Id { get; private set; }
    public Guid ClassificationId { get; private set; }
    public string EntityType { get; private set; }
    public string Value { get; private set; }
    public decimal Confidence { get; private set; }
    public int StartPosition { get; private set; }
    public int EndPosition { get; private set; }

    // Navigation property
    public DocumentClassification Classification { get; private set; } = null!;

    private EntityExtraction() { } // For EF Core

    public EntityExtraction(Guid classificationId, string entityType, string value, 
        decimal confidence, int startPosition, int endPosition)
    {
        Id = Guid.NewGuid();
        ClassificationId = classificationId;
        EntityType = entityType ?? throw new ArgumentNullException(nameof(entityType));
        Value = value ?? throw new ArgumentNullException(nameof(value));
        Confidence = Math.Max(0, Math.Min(100, confidence));
        StartPosition = startPosition;
        EndPosition = endPosition;
    }
}
