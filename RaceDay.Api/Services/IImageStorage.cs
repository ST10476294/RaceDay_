namespace RaceDay.Api.Services;

// Controllers depend on this interface, so tests can swap in a fake instead of calling Azure.
public interface IImageStorage
{
    // False when no Azure connection string has been configured.
    bool IsConfigured { get; }

    // Saves the image in the given folder and returns its public URL.
    Task<string> UploadAsync(IFormFile file, string folder);
}
