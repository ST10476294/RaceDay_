using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using RaceDay.Mvc.Services;

namespace RaceDay.Mvc.Filters;

// Protects pages. Visitors who are not logged in go to Login, and users with the
// wrong role go to the Access Denied page. The API still enforces every rule itself.
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequireLoginAttribute(string? role = null) : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var user = context.HttpContext.RequestServices.GetRequiredService<UserSession>();
        if (!user.IsLoggedIn)
        {
            var request = context.HttpContext.Request;
            var returnUrl = request.Method == "GET" ? request.Path + request.QueryString : null;
            context.Result = new RedirectToActionResult("Login", "Account", new { returnUrl });
        }
        else if (role != null && user.Role != role)
        {
            context.Result = new RedirectToActionResult("AccessDenied", "Home", null);
        }
    }
}
