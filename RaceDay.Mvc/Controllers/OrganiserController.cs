using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Filters;
using RaceDay.Mvc.Models;
using RaceDay.Mvc.Services;

namespace RaceDay.Mvc.Controllers;

// All Organiser pages. The API confirms that the Organiser owns each record.
[RequireLogin("Organiser")]
public class OrganiserController(RaceDayApiClient api) : BaseController
{
    public async Task<IActionResult> Dashboard()
    {
        var result = await api.GetMyEventsAsync();
        return result.Success ? View(result.Data) : ApiFailure(result);
    }

    // ---------- Events ----------
    [HttpGet]
    public IActionResult Create() => View("EventForm", new EventFormViewModel { EventDate = DateTime.Today.AddDays(30).AddHours(7) });

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(EventFormViewModel model)
    {
        if (!ModelState.IsValid) return View("EventForm", model);
        var result = await api.CreateEventAsync(model.ToApiBody());
        if (!result.Success)
        {
            AddApiErrors(result);
            return View("EventForm", model);
        }
        Success("Event created. Now add its categories.");
        var id = result.Data!.EventId;
        return await UploadBannerThenRedirect(id, model.Banner, RedirectToAction(nameof(Categories), new { id }));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var result = await api.GetEventAsync(id);
        if (!result.Success) return ApiFailure(result);
        var e = result.Data!;
        return View("EventForm", new EventFormViewModel
        {
            EventId = e.EventId, Name = e.Name, Discipline = e.Discipline, EventDate = e.EventDate, Location = e.Location,
            DistanceKm = e.DistanceKm, Description = e.Description ?? "", BannerImageUrl = e.BannerImageUrl
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, EventFormViewModel model)
    {
        model.EventId = id;
        if (!ModelState.IsValid) return View("EventForm", model);
        var result = await api.UpdateEventAsync(id, model.ToApiBody());
        if (!result.Success)
        {
            AddApiErrors(result);
            return View("EventForm", model);
        }
        Success("Event updated.");
        return await UploadBannerThenRedirect(id, model.Banner, RedirectToAction(nameof(Dashboard)));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await api.DeleteEventAsync(id);
        if (result.Success) Success("Event deleted.");
        else Error(result.Error ?? "The event could not be deleted.");
        return RedirectToAction(nameof(Dashboard));
    }

    // The event is already saved here, so a failed upload is reported without losing it.
    private async Task<IActionResult> UploadBannerThenRedirect(int id, IFormFile? banner, IActionResult next)
    {
        if (banner == null) return next;
        var upload = await api.UploadBannerAsync(id, banner);
        if (upload.Success) return next;
        Error($"The event was saved, but the banner was not: {upload.Error}");
        return RedirectToAction(nameof(Edit), new { id });
    }

    // ---------- Categories ----------
    [HttpGet]
    public async Task<IActionResult> Categories(int id)
    {
        var result = await api.GetEventAsync(id);
        return result.Success ? View(new CategoriesViewModel { Event = result.Data! }) : ApiFailure(result);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AddCategory(int id, [Bind(Prefix = "NewCategory")] CategoryFormViewModel model)
    {
        var page = await api.GetEventAsync(id);
        if (!page.Success) return ApiFailure(page);
        if (ModelState.IsValid)
        {
            var result = await api.CreateCategoryAsync(id, model.ToApiBody());
            if (result.Success)
            {
                Success($"{model.Name} added.");
                return RedirectToAction(nameof(Categories), new { id });
            }
            AddApiErrors(result);
        }
        return View(nameof(Categories), new CategoriesViewModel { Event = page.Data!, NewCategory = model });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateCategory(int id, CategoryFormViewModel model)
    {
        if (!ModelState.IsValid) Error("Fill in every category field.");
        else
        {
            var result = await api.UpdateCategoryAsync(model.CategoryId ?? 0, model.ToApiBody());
            if (result.Success) Success($"{model.Name} updated.");
            else Error(result.FieldErrors.Values.SelectMany(m => m).FirstOrDefault() ?? result.Error ?? "The category could not be updated.");
        }
        return RedirectToAction(nameof(Categories), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCategory(int id, int categoryId)
    {
        var result = await api.DeleteCategoryAsync(categoryId);
        if (result.Success) Success("Category removed.");
        else Error(result.Error ?? "The category could not be removed.");
        return RedirectToAction(nameof(Categories), new { id });
    }

    // ---------- Enrolments and results ----------
    public Task<IActionResult> Enrolments(int id) => EventWithEnrolments(id);

    public Task<IActionResult> Results(int id) => EventWithEnrolments(id);

    private async Task<IActionResult> EventWithEnrolments(int id)
    {
        var e = await api.GetEventAsync(id);
        if (!e.Success) return ApiFailure(e);
        var rows = await api.GetEventEnrolmentsAsync(id);
        if (!rows.Success) return ApiFailure(rows);
        return View(new EventEnrolmentsViewModel { Event = e.Data!, Enrolments = rows.Data! });
    }

    // Saving publishes the result: the Participant can see it straight away on My Results.
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveResult(ResultFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            Error("Enter the finish time as hh:mm:ss, for example 01:12:45.");
            return RedirectToAction(nameof(Results), new { id = model.EventId });
        }
        var result = model.ResultId is { } resultId
            ? await api.UpdateResultAsync(resultId, model.ToApiBody())
            : await api.CreateResultAsync(model.EnrolmentId, model.ToApiBody());
        if (result.Success) Success("Result saved and published.");
        else Error(result.FieldErrors.Values.SelectMany(m => m).FirstOrDefault() ?? result.Error ?? "The result could not be saved.");
        return RedirectToAction(nameof(Results), new { id = model.EventId });
    }
}
