using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Briefcase.Core.Interfaces;

namespace Briefcase.Infrastructure.Services;
public class BlobStorageService : IBlobStorageService
{
    private readonly BlobServiceClient _blobServiceClient;

    public BlobStorageService(IKeyVaultService keyVaultService)
    {
        var connectionString = keyVaultService.GetSecret("storage-acc-conn-str-1");
        _blobServiceClient = new BlobServiceClient(connectionString);
    }

    public async Task UploadAsync(string containerName, string blobName, Stream content, string contentType)
    {
        var containerClient = await GetOrCreateContainerAsync(containerName);
        var blobClient = containerClient.GetBlobClient(blobName);
        var blobHttpHeaders = new BlobHttpHeaders { ContentType = contentType };
        await blobClient.UploadAsync(content, new BlobUploadOptions { HttpHeaders = blobHttpHeaders });
    }

    public async Task<Stream> DownloadAsync(string containerName, string blobName)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(blobName);

        if(!await blobClient.ExistsAsync())
            throw new FileNotFoundException($"Blob '{blobName}' not found in container '{containerName}'.");

        var response = await blobClient.DownloadStreamingAsync();
        return response.Value.Content;
    }

    public async Task<bool> DeleteAsync(string containerName, string blobName)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(blobName);
        var response = await blobClient.DeleteIfExistsAsync();
        return response.Value;
    }

    public async Task<bool> ExistsAsync(string containerName, string blobName)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(blobName);
        return await blobClient.ExistsAsync();
    }

    private async Task<BlobContainerClient> GetOrCreateContainerAsync(string containerName)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        await containerClient.CreateIfNotExistsAsync(PublicAccessType.None);
        return containerClient;
    }

    public string GetBlobServiceUri()
    {
        return _blobServiceClient.Uri.ToString();
    }
}