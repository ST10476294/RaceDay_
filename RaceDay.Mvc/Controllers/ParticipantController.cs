using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Filters;
using RaceDay.Mvc.Services;

namespace RaceDay.Mvc.Controllers;

[RequireLogin("Participant")]
public class ParticipantController(RaceDayApiClient api, UserSession user) : BaseController
{
    public async Task<IActionResult> MyEvents()
    {
        var result = await api.GetMyEnrolmentsAsync(user.UserId!.Value);
        return result.Success ? View(result.Data) : ApiFailure(result);
    }

    public async Task<IActionResult> MyResults()
    {
        var result = await api.GetMyEnrolmentsAsync(user.UserId!.Value);
        return result.Success ? View(result.Data!.Where(e => e.Result != null).ToList()) : ApiFailure(result);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        var result = await api.CancelEnrolmentAsync(id);
        if (result.Success) Success("Your entry has been cancelled.");
        else Error(result.Error ?? "Your entry could not be cancelled.");
        return RedirectToAction(nameof(MyEvents));
    }
}
