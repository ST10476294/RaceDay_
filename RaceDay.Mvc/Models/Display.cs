using RaceDay.Mvc.Models.Api;

namespace RaceDay.Mvc.Models;

// Small formatting helpers used by the views.
public static class Display
{
    public static string Date(DateTime value) => value.ToString("ddd, d MMM yyyy");
    public static string DateTimeText(DateTime value) => value.ToString("ddd, d MMM yyyy, HH:mm");
    public static string Km(decimal value) => $"{value:0.##} km";
    public static string Rand(decimal value) => value == 0 ? "Free" : $"R{value:0.00}";
    public static string Time(TimeSpan? value) => value?.ToString(@"hh\:mm\:ss") ?? "-";

    // 1 -> 1st, 2 -> 2nd, 3 -> 3rd, 11 -> 11th, 47 -> 47th
    public static string Ordinal(int number)
    {
        var lastTwo = number % 100;
        if (lastTwo is 11 or 12 or 13) return $"{number}th";
        return (number % 10) switch
        {
            1 => $"{number}st",
            2 => $"{number}nd",
            3 => $"{number}rd",
            _ => $"{number}th"
        };
    }

    // Colour coding: green Confirmed, amber Pending, red Cancelled.
    public static string StatusClass(string status) => status switch
    {
        "Confirmed" or "Finished" => "status-confirmed",
        "Pending" => "status-pending",
        _ => "status-cancelled"
    };

    // Events without an uploaded banner use the artwork for their discipline.
    public static string Banner(EventDto e) => e.BannerImageUrl ?? e.Discipline switch
    {
        "Cycling" => "/img/cycling.svg",
        "Walking" => "/img/walking.svg",
        _ => "/img/running.svg"
    };
}
