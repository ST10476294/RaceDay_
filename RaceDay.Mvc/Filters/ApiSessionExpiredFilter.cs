using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using RaceDay.Mvc.Services;

namespace RaceDay.Mvc.Filters;

// If the API session has ended, clear the website session too and ask the user to log in again.
public class ApiSessionExpiredFilter(UserSession user, ITempDataDictionaryFactory tempDataFactory) : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is not ApiSessionExpiredException) return;
        user.Clear();
        tempDataFactory.GetTempData(context.HttpContext)["Error"] = "Your session has ended. Please log in again.";
        context.Result = new RedirectToActionResult("Login", "Account", null);
        context.ExceptionHandled = true;
    }
}
