using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Models;
using System.Data;
namespace RaceDay.Api.Controllers;
public class EnrolmentsController(RaceDayDbContext db) : SessionController
{
    /// <summary>Enter a category as the logged-in Participant; reject full, duplicate or past entries.</summary>
    [HttpPost("api/categories/{id:int}/enrolments")]
    [ProducesResponseType(typeof(EnrolmentResponse),201)]
    public async Task<IActionResult> Create(int id)
    {
        if (RequireSession("Participant") is { } error) return error;
        // Keep capacity checking and insertion in one transaction.
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var c = await db.Categories.Include(c => c.Event).SingleOrDefaultAsync(c => c.CategoryId == id);
        if (c == null) return NotFound();
        if (c.Event!.EventDate <= DateTime.Now) return Conflict(new { message = "Event has already started." });
        if (await db.Enrolments.AnyAsync(e => e.ParticipantId == UserId && e.CategoryId == id))
            return Conflict(new { message = "Already enrolled in this category, including cancelled entries." });
        if (await db.Enrolments.CountAsync(e => e.CategoryId == id && e.Status != "Cancelled") >= c.MaxParticipants)
            return Conflict(new { message = "Category is full." });
        var entry = new Enrolment { ParticipantId=UserId, CategoryId=id, Category=c };
        db.Enrolments.Add(entry);
        try { await db.SaveChangesAsync(); await transaction.CommitAsync(); }
        catch (DbUpdateException) { return Conflict(new { message = "Enrolment conflicts with existing data. Retry the request." }); }
        return Created($"/api/users/{UserId}/enrolments",Responses.Enrolment(entry));
    }
    /// <summary>Cancel your enrolment before the event; retain its history.</summary>
    [HttpDelete("api/enrolments/{id:int}")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> Cancel(int id)
    {
        if (RequireSession("Participant") is { } error) return error;
        var e = await db.Enrolments.Include(e => e.Category)!.ThenInclude(c => c!.Event).SingleOrDefaultAsync(e => e.EnrolmentId == id);
        if (e == null) return NotFound(); if (e.ParticipantId != UserId) return Forbidden();
        if (e.Category!.Event!.EventDate <= DateTime.Now) return Conflict(new { message = "Cancellation is only allowed before the event." });
        e.Status="Cancelled"; await db.SaveChangesAsync(); return NoContent();
    }
}
