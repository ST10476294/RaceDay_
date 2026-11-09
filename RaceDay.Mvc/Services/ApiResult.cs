namespace RaceDay.Mvc.Services;

// The outcome of an API call, so controllers can show a friendly message instead of crashing.
public class ApiResult<T>
{
    public bool Success { get; init; }
    public int StatusCode { get; init; }
    public T? Data { get; init; }
    public string? Error { get; init; }

    // Validation problems from the API, by field name (for example "Name").
    public Dictionary<string, string[]> FieldErrors { get; init; } = [];
}

// Thrown when the API says the user is no longer logged in. See ApiSessionExpiredFilter.
public class ApiSessionExpiredException : Exception;
