using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using RaceDay.Mvc.Models.Api;

namespace RaceDay.Mvc.Services;

// The only class that talks to the Part 2 API. The MVC app has no database or Azure access.
public class RaceDayApiClient(HttpClient http, UserSession session, ILogger<RaceDayApiClient> logger)
{
    private const string ApiCookieName = "RaceDay.Session";
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    // ---------- Authentication ----------
    public async Task<ApiResult<LoginResponse>> LoginAsync(string email, string password)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "api/auth/login")
        { Content = JsonContent.Create(new { email, password }) };
        return await SendAsync<LoginResponse>(request, captureCookie: true);
    }
    public Task<ApiResult<RegisterResponse>> RegisterAsync(object body) => PostAsync<RegisterResponse>("api/auth/register", body);
    public Task<ApiResult<object>> LogoutAsync() => PostAsync<object>("api/auth/logout", null);

    // ---------- Events ----------
    public Task<ApiResult<List<EventDto>>> GetEventsAsync(string? discipline, string? location) =>
        GetAsync<List<EventDto>>($"api/events?discipline={Uri.EscapeDataString(discipline ?? "")}&location={Uri.EscapeDataString(location ?? "")}");
    public Task<ApiResult<EventDto>> GetEventAsync(int id) => GetAsync<EventDto>($"api/events/{id}");
    public Task<ApiResult<List<OrganiserEventDto>>> GetMyEventsAsync() => GetAsync<List<OrganiserEventDto>>("api/events/mine");
    public Task<ApiResult<EventDto>> CreateEventAsync(object body) => PostAsync<EventDto>("api/events", body);
    public Task<ApiResult<EventDto>> UpdateEventAsync(int id, object body) => PutAsync<EventDto>($"api/events/{id}", body);
    public Task<ApiResult<object>> DeleteEventAsync(int id) => DeleteAsync($"api/events/{id}");
    public Task<ApiResult<EventDto>> UploadBannerAsync(int id, IFormFile file) => UploadAsync<EventDto>($"api/events/{id}/banner", file);
    public Task<ApiResult<List<EnrolmentDto>>> GetEventEnrolmentsAsync(int id) => GetAsync<List<EnrolmentDto>>($"api/events/{id}/enrolments");

    // ---------- Categories ----------
    public Task<ApiResult<CategoryDto>> CreateCategoryAsync(int eventId, object body) => PostAsync<CategoryDto>($"api/events/{eventId}/categories", body);
    public Task<ApiResult<CategoryDto>> UpdateCategoryAsync(int id, object body) => PutAsync<CategoryDto>($"api/categories/{id}", body);
    public Task<ApiResult<object>> DeleteCategoryAsync(int id) => DeleteAsync($"api/categories/{id}");

    // ---------- Enrolments ----------
    public Task<ApiResult<EnrolmentDto>> EnrolAsync(int categoryId) => PostAsync<EnrolmentDto>($"api/categories/{categoryId}/enrolments", null);
    public Task<ApiResult<object>> CancelEnrolmentAsync(int id) => DeleteAsync($"api/enrolments/{id}");
    public Task<ApiResult<List<EnrolmentDto>>> GetMyEnrolmentsAsync(int userId) => GetAsync<List<EnrolmentDto>>($"api/users/{userId}/enrolments");

    // ---------- Results ----------
    public Task<ApiResult<ResultDto>> CreateResultAsync(int enrolmentId, object body) => PostAsync<ResultDto>($"api/enrolments/{enrolmentId}/results", body);
    public Task<ApiResult<ResultDto>> UpdateResultAsync(int resultId, object body) => PutAsync<ResultDto>($"api/results/{resultId}", body);

    // ---------- Profile ----------
    public Task<ApiResult<ProfileDto>> GetProfileAsync(int id) => GetAsync<ProfileDto>($"api/users/{id}");
    public Task<ApiResult<ProfileDto>> UpdateProfileAsync(int id, object body) => PutAsync<ProfileDto>($"api/users/{id}", body);
    public Task<ApiResult<ProfileDto>> UploadProfilePictureAsync(int id, IFormFile file) => UploadAsync<ProfileDto>($"api/users/{id}/profile-picture", file);

    // ---------- Shared request helpers ----------
    private Task<ApiResult<T>> GetAsync<T>(string path) => SendAsync<T>(new HttpRequestMessage(HttpMethod.Get, path));

    private Task<ApiResult<T>> PostAsync<T>(string path, object? body) => SendAsync<T>(
        new HttpRequestMessage(HttpMethod.Post, path) { Content = body == null ? null : JsonContent.Create(body) });

    private Task<ApiResult<T>> PutAsync<T>(string path, object body) => SendAsync<T>(
        new HttpRequestMessage(HttpMethod.Put, path) { Content = JsonContent.Create(body) });

    private Task<ApiResult<object>> DeleteAsync(string path) => SendAsync<object>(new HttpRequestMessage(HttpMethod.Delete, path));

    // Forwards the uploaded file to the API. The API, not this app, stores it in Azure.
    private async Task<ApiResult<T>> UploadAsync<T>(string path, IFormFile file)
    {
        await using var stream = file.OpenReadStream();
        using var content = new MultipartFormDataContent();
        var filePart = new StreamContent(stream);
        filePart.Headers.ContentType = new MediaTypeHeaderValue(string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType);
        content.Add(filePart, "file", file.FileName);
        return await SendAsync<T>(new HttpRequestMessage(HttpMethod.Post, path) { Content = content });
    }

    private async Task<ApiResult<T>> SendAsync<T>(HttpRequestMessage request, bool captureCookie = false)
    {
        // Prove to the API who this visitor is by sending back their API session cookie.
        var cookie = session.ApiCookie;
        if (cookie != null) request.Headers.Add("Cookie", $"{ApiCookieName}={cookie}");

        try
        {
            using var response = await http.SendAsync(request);
            if (captureCookie && response.IsSuccessStatusCode) session.ApiCookie = ReadApiCookie(response);

            // A 401 on a logged-in call means the API session ended, for example after a restart.
            if (response.StatusCode == HttpStatusCode.Unauthorized && !captureCookie) throw new ApiSessionExpiredException();
            return await ReadResultAsync<T>(response);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            logger.LogError(ex, "Could not reach the RaceDay API.");
            return new ApiResult<T> { StatusCode = 503, Error = "RaceDay is temporarily unavailable. Please try again shortly." };
        }
    }

    private static string? ReadApiCookie(HttpResponseMessage response)
    {
        if (!response.Headers.TryGetValues("Set-Cookie", out var cookies)) return null;
        var prefix = ApiCookieName + "=";
        var match = cookies.FirstOrDefault(c => c.StartsWith(prefix, StringComparison.Ordinal));
        return match?[prefix.Length..].Split(';')[0];
    }

    private static async Task<ApiResult<T>> ReadResultAsync<T>(HttpResponseMessage response)
    {
        var status = (int)response.StatusCode;
        var text = await response.Content.ReadAsStringAsync();
        if (response.IsSuccessStatusCode)
        {
            var data = string.IsNullOrWhiteSpace(text) ? default : JsonSerializer.Deserialize<T>(text, Json);
            return new ApiResult<T> { Success = true, StatusCode = status, Data = data };
        }
        var (message, fields) = ReadError(text);
        return new ApiResult<T> { StatusCode = status, Error = message ?? FriendlyMessage(status), FieldErrors = fields };
    }

    // The API sends { message } for business rules and { errors } for validation failures.
    private static (string? Message, Dictionary<string, string[]> Fields) ReadError(string text)
    {
        var fields = new Dictionary<string, string[]>();
        try
        {
            using var doc = JsonDocument.Parse(text);
            string? message = doc.RootElement.TryGetProperty("message", out var m) ? m.GetString() : null;
            if (doc.RootElement.TryGetProperty("errors", out var errors) && errors.ValueKind == JsonValueKind.Object)
                foreach (var field in errors.EnumerateObject())
                    fields[field.Name] = field.Value.EnumerateArray().Select(e => e.GetString() ?? "").ToArray();
            return (message, fields);
        }
        catch (JsonException) { return (null, fields); }
    }

    private static string FriendlyMessage(int status) => status switch
    {
        400 => "Please check the details you entered.",
        403 => "You do not have permission to do that.",
        404 => "That record could not be found.",
        409 => "That change conflicts with existing data.",
        _ => "Something went wrong. Please try again."
    };
}
