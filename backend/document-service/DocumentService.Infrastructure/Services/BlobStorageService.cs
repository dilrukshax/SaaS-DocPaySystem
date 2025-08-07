using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using DocumentService.Application.Interfaces;

namespace DocumentService.Infrastructure.Services;

public class BlobStorageService : IBlobStorageService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly string _containerName;

    public BlobStorageService(BlobServiceClient blobServiceClient, string containerName = "documents")
    {
        _blobServiceClient = blobServiceClient;
        _containerName = containerName;
    }

    public async Task<string> UploadAsync(
        Stream fileStream, 
        string fileName, 
        string mimeType, 
        CancellationToken cancellationToken = default)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
        await containerClient.CreateIfNotExistsAsync(PublicAccessType.None, cancellationToken: cancellationToken);

        var blobName = $"{Guid.NewGuid()}/{fileName}";
        var blobClient = containerClient.GetBlobClient(blobName);

        var headers = new BlobHttpHeaders
        {
            ContentType = mimeType
        };

        await blobClient.UploadAsync(
            fileStream, 
            new BlobUploadOptions
            {
                HttpHeaders = headers,
                Conditions = null
            }, 
            cancellationToken);

        return blobName;
    }

    public async Task<Stream> DownloadAsync(string storagePath, CancellationToken cancellationToken = default)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
        var blobClient = containerClient.GetBlobClient(storagePath);

        var response = await blobClient.DownloadStreamingAsync(cancellationToken: cancellationToken);
        return response.Value.Content;
    }

    public async Task<string> GenerateDownloadUrlAsync(
        string storagePath, 
        TimeSpan expiry, 
        CancellationToken cancellationToken = default)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
        var blobClient = containerClient.GetBlobClient(storagePath);

        if (blobClient.CanGenerateSasUri)
        {
            var sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = _containerName,
                BlobName = storagePath,
                Resource = "b",
                ExpiresOn = DateTimeOffset.UtcNow.Add(expiry)
            };

            sasBuilder.SetPermissions(BlobSasPermissions.Read);

            return blobClient.GenerateSasUri(sasBuilder).ToString();
        }

        // Fallback for development/testing
        return blobClient.Uri.ToString();
    }

    public async Task DeleteAsync(string storagePath, CancellationToken cancellationToken = default)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
        var blobClient = containerClient.GetBlobClient(storagePath);

        await blobClient.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots, cancellationToken: cancellationToken);
    }

    public async Task<bool> ExistsAsync(string storagePath, CancellationToken cancellationToken = default)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
        var blobClient = containerClient.GetBlobClient(storagePath);

        var response = await blobClient.ExistsAsync(cancellationToken);
        return response.Value;
    }
}
