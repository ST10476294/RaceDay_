using Azure;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Api.Services;
namespace RaceDay.Api.Controllers;
[ApiController]
[Produces("application/json")]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
[ProducesResponseType(StatusCodes.Status409Conflict)]
public abstract class SessionController : ControllerBase
{
    protected int UserId => HttpContext.Session.GetInt32("UserId") ?? 0;
    protected IActionResult? RequireSession(string? role = null)
    {
        if (UserId == 0) return Unauthorized(new { message = "Log in first." });
        if (role != null && HttpContext.Session.GetString("Role") != role)
            return StatusCode(403, new { message = "Your role cannot perform this action." });
        return null;
    }
    // Shared by the banner and profile picture endpoints. Returns the URL, or the error to send back.
    protected async Task<(string? Url, IActionResult? Failure)> UploadImage(IImageStorage images, IFormFile file, string folder)
    {
        if (!images.IsConfigured)
            return (null, StatusCode(503, new { message = "Image storage is not configured." }));
        try { return (await images.UploadAsync(file, folder), null); }
        catch (RequestFailedException)
        { return (null, StatusCode(503, new { message = "The image could not be stored. Try again later." })); }
    }
    protected IActionResult Forbidden() => StatusCode(403, new { message = "This record belongs to another user." });
}
