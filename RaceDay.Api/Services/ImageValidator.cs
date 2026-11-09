namespace RaceDay.Api.Services;

// Checks an upload before it is sent to storage.
public static class ImageValidator
{
    public const long MaxBytes = 5 * 1024 * 1024; // 5 MB

    // Returns an error message, or null when the file is acceptable.
    public static string? Check(IFormFile? file)
    {
        if (file == null || file.Length == 0) return "Choose an image to upload.";
        if (file.Length > MaxBytes) return "Images must be 5 MB or smaller.";

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (extension is not (".jpg" or ".jpeg" or ".png" or ".webp"))
            return "Only JPG, PNG or WEBP images are allowed.";

        // The name can lie, so also check the first bytes of the file itself.
        var header = new byte[12];
        using var stream = file.OpenReadStream();
        var read = stream.Read(header, 0, header.Length);
        return read == header.Length && IsImage(header) ? null : "That file is not a valid image.";
    }

    private static bool IsImage(byte[] b)
    {
        bool jpeg = b[0] == 0xFF && b[1] == 0xD8 && b[2] == 0xFF;
        bool png = b[0] == 0x89 && b[1] == 0x50 && b[2] == 0x4E && b[3] == 0x47;
        bool webp = b[0] == 'R' && b[1] == 'I' && b[2] == 'F' && b[3] == 'F'
                 && b[8] == 'W' && b[9] == 'E' && b[10] == 'B' && b[11] == 'P';
        return jpeg || png || webp;
    }
}
