using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Models;
using System.Text;
namespace RaceDay.Api.Controllers;
[Route("api/auth")]
public class AuthController(RaceDayDbContext db, IConfiguration configuration) : SessionController
{
    /// <summary>Register a Participant, or an Organiser who supplies the valid organiser code.</summary>
    /// <response code="201">The new user ID.</response>
    /// <response code="403">Organiser registration needs a valid organiser code.</response>
    /// <response code="409">The email is already registered.</response>
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        if (dto.Role is not ("Organiser" or "Participant") || Encoding.UTF8.GetByteCount(dto.Password) > 72)
            return BadRequest(new { message = "Choose a valid role and a password of at most 72 UTF-8 bytes." });
        // The role comes from the client, so Organiser access must be proven with a server-side code.
        if (dto.Role == "Organiser" && !IsValidOrganiserCode(dto.OrganiserCode))
            return StatusCode(403, new { message = "A valid organiser code is required to register as an Organiser." });
        var email = dto.Email.Trim().ToLowerInvariant();
        if (await db.Users.AnyAsync(u => u.Email == email)) return Conflict(new { message = "Email already registered." });
        var user = new User { FullName = dto.FullName.Trim(), Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password), Role = dto.Role };
        db.Users.Add(user);
        try { await db.SaveChangesAsync(); }
        catch (DbUpdateException)
        {
            if (await db.Users.AsNoTracking().AnyAsync(u => u.Email == email))
                return Conflict(new { message = "Email already registered." });
            throw;
        }
        return Created($"/api/users/{user.UserId}", new { userId = user.UserId });
    }
    /// <summary>Log in; store user ID and role in a server-side session.</summary>
    /// <response code="200">User ID and role; session cookie is set.</response>
    /// <response code="401">Invalid email or password.</response>
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var email = dto.Email.Trim().ToLowerInvariant();
        var user = await db.Users.SingleOrDefaultAsync(u => u.Email == email);
        bool verified = false;
        if (user != null && Encoding.UTF8.GetByteCount(dto.Password) <= 72)
        {
            try { verified = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash); }
            catch (BCrypt.Net.SaltParseException) { /* Part 1 placeholder hashes cannot authenticate. */ }
        }
        if (!verified || user == null) return Unauthorized(new { message = "Invalid email or password." });
        HttpContext.Session.Clear();
        // The cookie identifies the session; authenticated details remain on the server.
        HttpContext.Session.SetInt32("UserId", user.UserId);
        HttpContext.Session.SetString("Role", user.Role);
        return Ok(new { userId = user.UserId, role = user.Role, fullName = user.FullName });
    }
    /// <summary>Clear the current server-side login session.</summary>
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        Response.Cookies.Delete("RaceDay.Session");
        return Ok(new { message = "Logged out successfully." });
    }
    private bool IsValidOrganiserCode(string? code)
    {
        var expected = configuration["Registration:OrganiserCode"];
        return !string.IsNullOrWhiteSpace(expected) && code == expected;
    }
}
