using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using RaceDay.Mvc.Services;

namespace RaceDay.Mvc.Controllers;

// Small helpers shared by the page controllers.
public abstract class BaseController : Controller
{
    protected void Success(string message) => TempData["Success"] = message;
    protected void Error(string message) => TempData["Error"] = message;

    // Shows API validation messages beside the matching form fields.
    protected void AddApiErrors<T>(ApiResult<T> result)
    {
        foreach (var (field, messages) in result.FieldErrors)
            foreach (var message in messages)
                ModelState.AddModelError(field.StartsWith('$') ? "" : field, message);
        if (result.FieldErrors.Count == 0) ModelState.AddModelError("", result.Error ?? "Something went wrong.");
    }

    // Shows "not found" and similar results on the friendly error page.
    protected IActionResult ApiFailure<T>(ApiResult<T> result) => RedirectToAction("Error", "Home", new { code = result.StatusCode });
}
