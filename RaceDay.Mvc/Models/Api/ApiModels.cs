namespace RaceDay.Mvc.Models.Api;

// Plain copies of the JSON the API returns. They hold data only, no business rules.
public record LoginResponse(int UserId, string Role, string FullName);
public record RegisterResponse(int UserId);
public record ProfileDto(int UserId, string FullName, string Email, string Role, string? ProfilePictureUrl);
public record CategoryDto(int CategoryId, int EventId, string Name, decimal DistanceKm, int MaxParticipants, decimal EntryFee);
public record RouteDto(int RouteId, string? Description, int? ElevationGainM, string? MapUrl);
public record EventDto(int EventId, int OrganiserId, string Name, string Discipline, DateTime EventDate, string Location,
    string? Description, decimal DistanceKm, string? BannerImageUrl, List<CategoryDto> Categories, List<RouteDto> Routes);
public record OrganiserEventDto(EventDto Event, int TotalEnrolments);
public record ResultDto(int ResultId, int EnrolmentId, TimeSpan? FinishTime, int? OverallPosition, int? CategoryPosition, string Status);
public record EnrolmentDto(int EnrolmentId, int ParticipantId, string? ParticipantName, string? ParticipantEmail, int EventId,
    string EventName, DateTime EventDate, int CategoryId, string CategoryName, string Status, ResultDto? Result, int TotalFinishers);
