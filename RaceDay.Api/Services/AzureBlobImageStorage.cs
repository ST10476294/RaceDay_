using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace RaceDay.Api.Services;

// Uploads images to Azure Blob Storage. Only the API talks to Azure; the MVC app never does.
public class AzureBlobImageStorage(IConfiguration configuration) : IImageStorage
{
    private readonly string? connectionString = configuration["AzureStorage:ConnectionString"];
    private readonly string containerName = configuration["AzureStorage:ContainerName"] ?? "raceday-images";

    public bool IsConfigured => !string.IsNullOrWhiteSpace(connectionString);

    public async Task<string> UploadAsync(IFormFile file, string folder)
    {
        var container = new BlobContainerClient(connectionString, containerName);
        // PublicAccessType.Blob lets the browser show an image from its URL without any key.
        await container.CreateIfNotExistsAsync(PublicAccessType.Blob);

        // A new GUID name stops two uploads with the same file name overwriting each other.
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var blob = container.GetBlobClient($"{folder}/{Guid.NewGuid()}{extension}");

        await using var stream = file.OpenReadStream();
        await blob.UploadAsync(stream, new BlobHttpHeaders { ContentType = file.ContentType });
        return blob.Uri.ToString();
    }
}
