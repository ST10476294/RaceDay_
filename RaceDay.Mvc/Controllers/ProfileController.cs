using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Filters;
using RaceDay.Mvc.Models;
using RaceDay.Mvc.Services;

namespace RaceDay.Mvc.Controllers;

[RequireLogin]
public class ProfileController(RaceDayApiClient api, UserSession user) : BaseController
{
    public async Task<IActionResult> Index()
    {
        var result = await api.GetProfileAsync(user.UserId!.Value);
        if (!result.Success) return ApiFailure(result);
        var p = result.Data!;
        return View(new ProfileFormViewModel { FullName = p.FullName, Email = p.Email, Role = p.Role, ProfilePictureUrl = p.ProfilePictureUrl });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(ProfileFormViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var result = await api.UpdateProfileAsync(user.UserId!.Value, new { model.FullName, model.Email });
        if (!result.Success)
        {
            AddApiErrors(result);
            return View(model);
        }
        user.UpdateName(result.Data!.FullName);
        Success("Your profile has been updated.");
        return RedirectToAction(nameof(Index));
    }

    // The picture goes to the API, which stores it in Azure Blob Storage.
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Picture(IFormFile? picture)
    {
        if (picture == null)
        {
            Error("Choose an image to upload.");
            return RedirectToAction(nameof(Index));
        }
        var result = await api.UploadProfilePictureAsync(user.UserId!.Value, picture);
        if (result.Success) Success("Your profile picture has been updated.");
        else Error(result.Error ?? "Your picture could not be uploaded.");
        return RedirectToAction(nameof(Index));
    }
}
