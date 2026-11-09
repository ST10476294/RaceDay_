using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Models;
namespace RaceDay.Api.Controllers;
public class ResultsController(RaceDayDbContext db) : SessionController
{
    /// <summary>View public category results ordered by finish time, with unfinished entries last.</summary>
    [HttpGet("api/categories/{id:int}/results")]
    [ProducesResponseType(typeof(IEnumerable<ResultResponse>),200)]
    public async Task<IActionResult> List(int id)
    {
        if (!await db.Categories.AnyAsync(c => c.CategoryId == id)) return NotFound();
        var results = await db.Results.Where(r => r.Enrolment!.CategoryId == id).ToListAsync();
        return Ok(results.OrderBy(r => r.Status != "Finished").ThenBy(r => r.FinishTime ?? TimeSpan.MaxValue).Select(Responses.Result));
    }
    /// <summary>Capture a result after the event for an enrolment in an event you own.</summary>
    [HttpPost("api/enrolments/{id:int}/results")]
    [ProducesResponseType(typeof(ResultResponse),201)]
    public async Task<IActionResult> Create(int id, ResultRequest request)
    {
        if (RequireSession("Organiser") is { } error) return error;
        var e = await db.Enrolments.Include(e => e.Category)!.ThenInclude(c => c!.Event).SingleOrDefaultAsync(e => e.EnrolmentId == id);
        if (e == null) return NotFound(); if (e.Category!.Event!.OrganiserId != UserId) return Forbidden();
        if (e.Category.Event.EventDate > DateTime.Now || e.Status == "Cancelled") return Conflict(new { message = "Result requires a past event and an active enrolment." });
        if (await db.Results.AnyAsync(r => r.EnrolmentId == id)) return Conflict(new { message = "Result exists; use PUT to correct it." });
        var r = new Result { EnrolmentId=id }; Apply(r,request); db.Results.Add(r);
        try { await db.SaveChangesAsync(); } catch (DbUpdateException) { return Conflict(new { message = "Result conflicts with existing data." }); }
        return Created($"/api/categories/{e.CategoryId}/results",Responses.Result(r));
    }
    /// <summary>Correct a result for an event you own.</summary>
    [HttpPut("api/results/{id:int}")]
    [ProducesResponseType(typeof(ResultResponse),200)]
    public async Task<IActionResult> Update(int id, ResultRequest request)
    {
        if (RequireSession("Organiser") is { } error) return error;
        var r = await db.Results.Include(r => r.Enrolment)!.ThenInclude(e => e!.Category)!.ThenInclude(c => c!.Event)
            .SingleOrDefaultAsync(r => r.ResultId == id);
        if (r == null) return NotFound(); if (r.Enrolment!.Category!.Event!.OrganiserId != UserId) return Forbidden();
        Apply(r,request); await db.SaveChangesAsync(); return Ok(Responses.Result(r));
    }
    private static void Apply(Result r, ResultRequest q)
    { r.FinishTime=q.FinishTime; r.OverallPosition=q.OverallPosition; r.CategoryPosition=q.CategoryPosition; r.Status=q.Status; }
}
