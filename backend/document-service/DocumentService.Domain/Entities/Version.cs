namespace DocumentService.Domain.Entities;

public class Version
{
    public Guid Id { get; private set; }
    public Guid DocumentId { get; private set; }
    public int VersionNumber { get; private set; }
    public string StoragePath { get; private set; }
    public long FileSize { get; private set; }
    public string? ChangeLog { get; private set; }
    public Guid CreatedBy { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public string? ChecksumHash { get; private set; }

    // Navigation property
    public Document Document { get; private set; } = null!;

    private Version() { } // For EF Core

    public Version(Guid documentId, int versionNumber, string storagePath, long fileSize, 
        Guid createdBy, string? changeLog = null)
    {
        Id = Guid.NewGuid();
        DocumentId = documentId;
        VersionNumber = versionNumber;
        StoragePath = storagePath ?? throw new ArgumentNullException(nameof(storagePath));
        FileSize = fileSize;
        ChangeLog = changeLog;
        CreatedBy = createdBy;
        CreatedAt = DateTime.UtcNow;
    }

    public void SetChecksum(string checksumHash)
    {
        ChecksumHash = checksumHash;
    }
}
