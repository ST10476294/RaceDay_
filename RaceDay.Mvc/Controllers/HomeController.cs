using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Models;
using RaceDay.Mvc.Services;

namespace RaceDay.Mvc.Controllers;

public class HomeController(RaceDayApiClient api) : BaseController
{
    // Upcoming events with filters. Filtering is done by the API.
    public async Task<IActionResult> Index(string? discipline, string? location)
    {
        var events = await api.GetEventsAsync(discipline, location);
        if (!events.Success) Error(events.Error ?? "Events could not be loaded.");
        return View(new HomeViewModel { Events = events.Data ?? [], Discipline = discipline, Location = location });
    }

    public IActionResult AccessDenied() => View();

    [IgnoreAntiforgeryToken]
    public IActionResult Error(int? code)
    {
        // The exception handler re-runs this action, so any real exception stays out of the page.
        Response.StatusCode = code ?? 500;
        return View(new ErrorViewModel { Code = code });
    }
}
