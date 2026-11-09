namespace RaceDay.Mvc.Services;

// One place for reading and writing the logged-in user's details in the MVC session.
public class UserSession(IHttpContextAccessor accessor)
{
    private ISession Session => accessor.HttpContext!.Session;

    public int? UserId => Session.GetInt32("UserId");
    public string? Role => Session.GetString("Role");
    public string? FullName => Session.GetString("FullName");
    public bool IsLoggedIn => UserId != null;
    public bool IsOrganiser => Role == "Organiser";
    public bool IsParticipant => Role == "Participant";

    // The API's own session cookie, kept here so it is never exposed to the browser.
    public string? ApiCookie
    {
        get => Session.GetString("ApiCookie");
        set { if (value != null) Session.SetString("ApiCookie", value); }
    }

    public void SignIn(int userId, string role, string fullName)
    {
        Session.SetInt32("UserId", userId);
        Session.SetString("Role", role);
        Session.SetString("FullName", fullName);
    }

    public void UpdateName(string fullName) => Session.SetString("FullName", fullName);

    public void Clear() => Session.Clear();
}
