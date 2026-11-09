using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Models;
namespace RaceDay.Api.Controllers;
public class CategoriesController(RaceDayDbContext db) : SessionController
{
    /// <summary>View the categories and distances for an event.</summary>
    [HttpGet("api/events/{id:int}/categories")]
    [ProducesResponseType(typeof(IEnumerable<CategoryResponse>),200)]
    public async Task<IActionResult> List(int id)
    {
        if (!await db.Events.AnyAsync(e => e.EventId == id)) return NotFound();
        return Ok((await db.Categories.Where(c => c.EventId == id).ToListAsync()).Select(Responses.Category));
    }
    /// <summary>Add a distance or age category to an event you own.</summary>
    [HttpPost("api/events/{id:int}/categories")]
    [ProducesResponseType(typeof(CategoryResponse),201)]
    public async Task<IActionResult> Create(int id, CategoryRequest r)
    {
        if (RequireSession("Organiser") is { } error) return error;
        var e = await db.Events.FindAsync(id);
        if (e == null) return NotFound(); if (e.OrganiserId != UserId) return Forbidden();
        var c = new Category { EventId=id }; Apply(c,r); db.Categories.Add(c); await db.SaveChangesAsync();
        return Created($"/api/events/{id}/categories",Responses.Category(c));
    }
    /// <summary>Update an owned category; capacity cannot drop below active enrolments.</summary>
    [HttpPut("api/categories/{id:int}")]
    [ProducesResponseType(typeof(CategoryResponse),200)]
    public async Task<IActionResult> Update(int id, CategoryRequest r)
    {
        if (RequireSession("Organiser") is { } error) return error;
        var c = await db.Categories.Include(c => c.Event).SingleOrDefaultAsync(c => c.CategoryId == id);
        if (c == null) return NotFound(); if (c.Event!.OrganiserId != UserId) return Forbidden();
        if (await db.Enrolments.CountAsync(e => e.CategoryId == id && e.Status != "Cancelled") > r.MaxParticipants)
            return Conflict(new { message = "Capacity is below current enrolments." });
        Apply(c,r); await db.SaveChangesAsync(); return Ok(Responses.Category(c));
    }
    /// <summary>Delete an owned category only when it has no enrolment history.</summary>
    [HttpDelete("api/categories/{id:int}")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> Delete(int id)
    {
        if (RequireSession("Organiser") is { } error) return error;
        var c = await db.Categories.Include(c => c.Event).SingleOrDefaultAsync(c => c.CategoryId == id);
        if (c == null) return NotFound(); if (c.Event!.OrganiserId != UserId) return Forbidden();
        if (await db.Enrolments.AnyAsync(e => e.CategoryId == id)) return Conflict(new { message = "Category has enrolment history." });
        db.Categories.Remove(c);
        try { await db.SaveChangesAsync(); } catch (DbUpdateException) { return Conflict(new { message = "Category is still in use." }); }
        return NoContent();
    }
    private static void Apply(Category c, CategoryRequest r)
    { c.Name=r.Name.Trim(); c.DistanceKm=r.DistanceKm; c.MaxParticipants=r.MaxParticipants; c.EntryFee=r.EntryFee; }
}
