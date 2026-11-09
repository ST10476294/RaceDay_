using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Services;
namespace RaceDay.Api.Controllers;
[Route("api/users")]
public class UsersController(RaceDayDbContext db, IImageStorage images) : SessionController
{
    /// <summary>View your own profile. Password hashes are never returned.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ProfileResponse),200)]
    public async Task<IActionResult> Get(int id)
    {
        if (RequireSession() is { } error) return error;
        if (id != UserId) return Forbidden();
        var u = await db.Users.FindAsync(id);
        return u == null ? NotFound() : Ok(Responses.Profile(u));
    }
    /// <summary>Update your own full name and email; role changes are not permitted.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ProfileResponse),200)]
    public async Task<IActionResult> Update(int id, UpdateProfileRequest request)
    {
        if (RequireSession() is { } error) return error;
        if (id != UserId) return Forbidden();
        var u = await db.Users.FindAsync(id);
        if (u == null) return NotFound();
        var email = request.Email.Trim().ToLowerInvariant();
        if (await db.Users.AnyAsync(x => x.Email == email && x.UserId != id)) return Conflict(new { message = "Email already registered." });
        u.FullName = request.FullName.Trim(); u.Email = email;
        try { await db.SaveChangesAsync(); }
        catch (DbUpdateException) { return Conflict(new { message = "Profile update conflicts with existing data." }); }
        return Ok(Responses.Profile(u));
    }
    /// <summary>Upload your own JPG, PNG or WEBP profile picture (max 5 MB) to Azure Blob Storage.</summary>
    /// <response code="200">The profile, including its new picture URL.</response>
    /// <response code="503">Image storage is not configured or could not be reached.</response>
    [HttpPost("{id:int}/profile-picture")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ProfileResponse),200)]
    [ProducesResponseType(503)]
    public async Task<IActionResult> UploadPicture(int id, IFormFile? file)
    {
        if (RequireSession() is { } error) return error;
        if (id != UserId) return Forbidden();
        var u = await db.Users.FindAsync(id);
        if (u == null) return NotFound();
        if (ImageValidator.Check(file) is { } problem) return BadRequest(new { message = problem });
        var (url, failure) = await UploadImage(images, file!, "profiles");
        if (failure != null) return failure;
        u.ProfilePictureUrl = url; await db.SaveChangesAsync();
        return Ok(Responses.Profile(u));
    }
    /// <summary>View your own enrolment history, including event details and personal results.</summary>
    [HttpGet("{id:int}/enrolments")]
    [ProducesResponseType(typeof(IEnumerable<EnrolmentResponse>),200)]
    public async Task<IActionResult> Enrolments(int id)
    {
        if (RequireSession("Participant") is { } error) return error;
        if (id != UserId) return Forbidden();
        var rows = await db.Enrolments.Include(e => e.Category)!.ThenInclude(c => c!.Event).Include(e => e.Result)
            .Where(e => e.ParticipantId == id).OrderByDescending(e => e.Category!.Event!.EventDate).ToListAsync();
        // Count finishers per event so a position can be shown as "47th out of 312".
        var eventIds = rows.Select(e => e.Category!.EventId).Distinct().ToList();
        var finishers = await db.Results.Where(r => r.Status == "Finished" && eventIds.Contains(r.Enrolment!.Category!.EventId))
            .GroupBy(r => r.Enrolment!.Category!.EventId).Select(g => new { EventId = g.Key, Total = g.Count() })
            .ToDictionaryAsync(x => x.EventId, x => x.Total);
        return Ok(rows.Select(e => Responses.Enrolment(e, finishers.GetValueOrDefault(e.Category!.EventId))));
    }
}
