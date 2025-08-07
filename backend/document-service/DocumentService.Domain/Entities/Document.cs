using DocumentService.Domain.ValueObjects;

namespace DocumentService.Domain.Entities;

public class Document
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public string FileName { get; private set; }
    public string MimeType { get; private set; }
    public long FileSize { get; private set; }
    public string StoragePath { get; private set; }
    public DocumentStatus Status { get; private set; }
    public int CurrentVersion { get; private set; }
    public Guid TenantId { get; private set; }
    public Guid CreatedBy { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public string? Tags { get; private set; }
    public bool IsDeleted { get; private set; }

    private readonly List<Version> _versions = new();
    public IReadOnlyList<Version> Versions => _versions.AsReadOnly();

    private readonly List<Metadata> _metadata = new();
    public IReadOnlyList<Metadata> Metadata => _metadata.AsReadOnly();

    private readonly List<AuditLog> _auditLogs = new();
    public IReadOnlyList<AuditLog> AuditLogs => _auditLogs.AsReadOnly();

    private Document() { } // For EF Core

    public Document(string name, string description, string fileName, string mimeType, 
        long fileSize, string storagePath, Guid tenantId, Guid createdBy, string? tags = null)
    {
        Id = Guid.NewGuid();
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description ?? string.Empty;
        FileName = fileName ?? throw new ArgumentNullException(nameof(fileName));
        MimeType = mimeType ?? throw new ArgumentNullException(nameof(mimeType));
        FileSize = fileSize;
        StoragePath = storagePath ?? throw new ArgumentNullException(nameof(storagePath));
        Status = DocumentStatus.Processing;
        CurrentVersion = 1;
        TenantId = tenantId;
        CreatedBy = createdBy;
        CreatedAt = DateTime.UtcNow;
        Tags = tags;
        IsDeleted = false;

        // Create initial version
        var initialVersion = new Version(Id, 1, storagePath, fileSize, createdBy, "Initial upload");
        _versions.Add(initialVersion);

        // Add audit log
        AddAuditLog("Document created", createdBy);
    }

    public void UpdateMetadata(string name, string description, string? tags, Guid updatedBy)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description ?? string.Empty;
        Tags = tags;
        UpdatedAt = DateTime.UtcNow;
        
        AddAuditLog($"Metadata updated", updatedBy);
    }

    public void AddVersion(string storagePath, long fileSize, Guid createdBy, string changeLog)
    {
        CurrentVersion++;
        var version = new Version(Id, CurrentVersion, storagePath, fileSize, createdBy, changeLog);
        _versions.Add(version);
        
        UpdatedAt = DateTime.UtcNow;
        AddAuditLog($"Version {CurrentVersion} added", createdBy);
    }

    public void AddMetadata(string key, string value, Guid createdBy)
    {
        var metadata = new Metadata(Id, key, value, createdBy);
        _metadata.Add(metadata);
        
        AddAuditLog($"Metadata added: {key}", createdBy);
    }

    public void UpdateStatus(DocumentStatus status, Guid updatedBy)
    {
        Status = status;
        UpdatedAt = DateTime.UtcNow;
        
        AddAuditLog($"Status changed to {status}", updatedBy);
    }

    public void SoftDelete(Guid deletedBy)
    {
        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
        
        AddAuditLog("Document deleted", deletedBy);
    }

    private void AddAuditLog(string action, Guid userId)
    {
        var auditLog = new AuditLog(Id, action, userId);
        _auditLogs.Add(auditLog);
    }
}
