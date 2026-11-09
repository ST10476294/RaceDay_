using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Filters;
using RaceDay.Mvc.Services;

namespace RaceDay.Mvc.Controllers;

public class EventsController(RaceDayApiClient api) : BaseController
{
    public async Task<IActionResult> Details(int id)
    {
        var result = await api.GetEventAsync(id);
        return result.Success ? View(result.Data) : ApiFailure(result);
    }

    // Records the enrolment through the API. The API checks capacity, duplicates and dates.
    [HttpPost, ValidateAntiForgeryToken, RequireLogin("Participant")]
    public async Task<IActionResult> Enter(int id, int? categoryId)
    {
        if (categoryId == null)
        {
            Error("Choose a category first.");
            return RedirectToAction(nameof(Details), new { id });
        }
        var result = await api.EnrolAsync(categoryId.Value);
        if (!result.Success)
        {
            Error(result.Error ?? "Your entry could not be recorded.");
            return RedirectToAction(nameof(Details), new { id });
        }
        Success($"You are entered in {result.Data!.EventName} ({result.Data.CategoryName}).");
        return RedirectToAction("MyEvents", "Participant");
    }
}
