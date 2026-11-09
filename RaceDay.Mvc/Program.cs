using System.Globalization;
using RaceDay.Mvc.Filters;
using RaceDay.Mvc.Services;

// Use one number format everywhere, so "21.1" from a form works on any computer or container,
// including those whose regional settings use a decimal comma.
CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

var builder = WebApplication.CreateBuilder(args);

// Every controller gets the session-expiry filter, so an API 401 always returns the user to login.
builder.Services.AddControllersWithViews(options => options.Filters.Add<ApiSessionExpiredFilter>());

// The MVC session remembers who is logged in between pages.
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.Name = "RaceDay.Web.Session";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.IdleTimeout = TimeSpan.FromMinutes(30);
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<UserSession>();

// UseCookies = false is essential: this HttpClient is shared by all website visitors,
// so it must never remember a cookie. RaceDayApiClient adds the correct one per request.
builder.Services.AddHttpClient<RaceDayApiClient>(client =>
    {
        client.BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"] ?? "http://localhost:5298");
        client.Timeout = TimeSpan.FromSeconds(30);
    })
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { UseCookies = false });

var app = builder.Build();

// Friendly error pages instead of raw exceptions, for failures and for unknown URLs.
app.UseExceptionHandler("/Home/Error");
app.UseStatusCodePagesWithReExecute("/Home/Error", "?code={0}");

app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
app.Run();
