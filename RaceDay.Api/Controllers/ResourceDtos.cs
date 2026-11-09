using System.ComponentModel.DataAnnotations;
namespace RaceDay.Api.Controllers;
public class UpdateProfileRequest
{
    [Required, StringLength(100)] public string FullName { get; set; } = "";
    [Required, EmailAddress, StringLength(150)] public string Email { get; set; } = "";
}
public class EventRequest : IValidatableObject
{
    [Required, StringLength(150)] public string Name { get; set; } = "";
    [Required, RegularExpression("^(Running|Walking|Cycling)$")] public string Discipline { get; set; } = "";
    public DateTime EventDate { get; set; }
    [Required, StringLength(150)] public string Location { get; set; } = "";
    [Required, StringLength(500)] public string Description { get; set; } = "";
    [Range(typeof(decimal), "0.01", "999.99")] public decimal DistanceKm { get; set; }
    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        if (EventDate <= DateTime.Now) yield return new ValidationResult("Event date must be in the future.", [nameof(EventDate)]);
    }
}
public class CategoryRequest
{
    [Required, StringLength(50)] public string Name { get; set; } = "";
    [Range(typeof(decimal), "0.01", "999.99")] public decimal DistanceKm { get; set; }
    [Range(1, int.MaxValue)] public int MaxParticipants { get; set; } = 100;
    [Range(typeof(decimal), "0", "999999.99")] public decimal EntryFee { get; set; }
}
public class ResultRequest : IValidatableObject
{
    public TimeSpan? FinishTime { get; set; }
    [Range(1, int.MaxValue)] public int? OverallPosition { get; set; }
    [Range(1, int.MaxValue)] public int? CategoryPosition { get; set; }
    [Required, RegularExpression("^(Finished|DNF|DSQ)$")] public string Status { get; set; } = "Finished";
    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        if (Status == "Finished" && (FinishTime == null || OverallPosition == null || CategoryPosition == null))
            yield return new ValidationResult("Finished results require a time and both positions.");
        if (FinishTime is { } time && (time <= TimeSpan.Zero || time >= TimeSpan.FromDays(1)))
            yield return new ValidationResult("Finish time must be positive and below 24 hours (Part 1 SQL TIME).");
    }
}
public record ProfileResponse(int UserId, string FullName, string Email, string Role, string? ProfilePictureUrl);
public record CategoryResponse(int CategoryId, int EventId, string Name, decimal DistanceKm, int MaxParticipants, decimal EntryFee);
public record RouteResponse(int RouteId, string? Description, int? ElevationGainM, string? MapUrl);
public record EventResponse(int EventId, int OrganiserId, string Name, string Discipline, DateTime EventDate,
    string Location, string? Description, decimal DistanceKm, string? BannerImageUrl,
    IEnumerable<CategoryResponse> Categories, IEnumerable<RouteResponse> Routes);
public record OrganiserEventResponse(EventResponse Event, int TotalEnrolments);
public record ResultResponse(int ResultId, int EnrolmentId, TimeSpan? FinishTime, int? OverallPosition, int? CategoryPosition, string Status);
public record EnrolmentResponse(int EnrolmentId, int ParticipantId, string? ParticipantName, string? ParticipantEmail,
    int EventId, string EventName, DateTime EventDate, int CategoryId, string CategoryName, string Status,
    ResultResponse? Result, int TotalFinishers);
internal static class Responses
{
    public static CategoryResponse Category(Models.Category c) => new(c.CategoryId,c.EventId,c.Name,c.DistanceKm,c.MaxParticipants,c.EntryFee);
    public static EventResponse Event(Models.Event e) => new(e.EventId,e.OrganiserId,e.Name,e.Discipline,e.EventDate,e.Location,e.Description,e.DistanceKm,e.BannerImageUrl,
        e.Categories.Select(Category), e.Routes.Select(r => new RouteResponse(r.RouteId,r.Description,r.ElevationGainM,r.MapUrl)));
    public static ResultResponse Result(Models.Result r) => new(r.ResultId,r.EnrolmentId,r.FinishTime,r.OverallPosition,r.CategoryPosition,r.Status);
    public static ProfileResponse Profile(Models.User u) => new(u.UserId,u.FullName,u.Email,u.Role,u.ProfilePictureUrl);
    // totalFinishers supports the "47th out of 312 finishers" display on My Results.
    public static EnrolmentResponse Enrolment(Models.Enrolment e, int totalFinishers = 0) => new(e.EnrolmentId,e.ParticipantId,
        e.Participant?.FullName,e.Participant?.Email,e.Category!.EventId,e.Category.Event!.Name,e.Category.Event.EventDate,
        e.CategoryId,e.Category.Name,e.Status,e.Result == null ? null : Result(e.Result),totalFinishers);
}
