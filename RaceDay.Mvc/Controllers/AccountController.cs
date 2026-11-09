using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Models;
using RaceDay.Mvc.Services;

namespace RaceDay.Mvc.Controllers;

public class AccountController(RaceDayApiClient api, UserSession user) : BaseController
{
    [HttpGet]
    public IActionResult Login(string? returnUrl) =>
        user.IsLoggedIn ? RedirectToLanding() : View(new LoginViewModel { ReturnUrl = returnUrl });

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var result = await api.LoginAsync(model.Email, model.Password);
        if (!result.Success)
        {
            ModelState.AddModelError("", result.Error ?? "Login failed.");
            return View(model);
        }
        user.SignIn(result.Data!.UserId, result.Data.Role, result.Data.FullName);
        // Url.IsLocalUrl stops a crafted link sending the user to another website after login.
        return Url.IsLocalUrl(model.ReturnUrl) ? Redirect(model.ReturnUrl!) : RedirectToLanding();
    }

    [HttpGet]
    public IActionResult Register() => user.IsLoggedIn ? RedirectToLanding() : View(new RegisterViewModel());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var result = await api.RegisterAsync(new { model.FullName, model.Email, model.Password, model.Role, model.OrganiserCode });
        if (!result.Success)
        {
            AddApiErrors(result);
            return View(model);
        }
        Success("Your account is ready. Log in to continue.");
        return RedirectToAction(nameof(Login));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        // End the API session first, then clear the website session.
        try { await api.LogoutAsync(); } catch (ApiSessionExpiredException) { /* Already ended. */ }
        user.Clear();
        Success("You have been logged out.");
        return RedirectToAction("Index", "Home");
    }

    private IActionResult RedirectToLanding() =>
        user.IsOrganiser ? RedirectToAction("Dashboard", "Organiser") : RedirectToAction("Index", "Home");
}
