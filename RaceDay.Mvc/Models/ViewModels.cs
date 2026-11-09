using System.ComponentModel.DataAnnotations;
using RaceDay.Mvc.Models.Api;

namespace RaceDay.Mvc.Models;

// Forms only check that boxes are filled in. Real rules stay in the API, and its messages are shown here.
public class LoginViewModel
{
    [Required(ErrorMessage = "Enter your email address."), EmailAddress(ErrorMessage = "Enter a valid email address.")]
    public string Email { get; set; } = "";
    [Required(ErrorMessage = "Enter your password."), DataType(DataType.Password)]
    public string Password { get; set; } = "";
    public string? ReturnUrl { get; set; }
}

public class RegisterViewModel
{
    [Required(ErrorMessage = "Enter your full name."), Display(Name = "Full name")]
    public string FullName { get; set; } = "";
    [Required(ErrorMessage = "Enter your email address."), EmailAddress(ErrorMessage = "Enter a valid email address.")]
    public string Email { get; set; } = "";
    [Required(ErrorMessage = "Choose a password."), DataType(DataType.Password)]
    public string Password { get; set; } = "";
    [Required(ErrorMessage = "Confirm your password."), DataType(DataType.Password), Display(Name = "Confirm password")]
    [Compare(nameof(Password), ErrorMessage = "The passwords do not match.")]
    public string ConfirmPassword { get; set; } = "";
    [Required(ErrorMessage = "Choose how you will use RaceDay.")]
    public string Role { get; set; } = "Participant";
    [Display(Name = "Organiser code")]
    public string? OrganiserCode { get; set; }
}

public class EventFormViewModel
{
    public int? EventId { get; set; }
    [Required(ErrorMessage = "Enter the event name."), Display(Name = "Event name")]
    public string Name { get; set; } = "";
    [Required(ErrorMessage = "Choose the event type."), Display(Name = "Event type")]
    public string Discipline { get; set; } = "Running";
    [Required(ErrorMessage = "Choose the event date."), Display(Name = "Date and start time")]
    public DateTime? EventDate { get; set; }
    [Required(ErrorMessage = "Enter the location.")]
    public string Location { get; set; } = "";
    [Required(ErrorMessage = "Enter the distance."), Display(Name = "Distance (km)")]
    public decimal? DistanceKm { get; set; }
    [Required(ErrorMessage = "Enter a description.")]
    public string Description { get; set; } = "";
    [Display(Name = "Banner image")]
    public IFormFile? Banner { get; set; }
    public string? BannerImageUrl { get; set; }

    public object ToApiBody() => new { Name, Discipline, EventDate, Location, Description, DistanceKm };
}

public class CategoryFormViewModel
{
    public int? CategoryId { get; set; }
    [Required(ErrorMessage = "Enter the category name.")]
    public string Name { get; set; } = "";
    [Required(ErrorMessage = "Enter the distance."), Display(Name = "Distance (km)")]
    public decimal? DistanceKm { get; set; }
    [Required(ErrorMessage = "Enter the capacity."), Display(Name = "Capacity")]
    public int? MaxParticipants { get; set; } = 100;
    [Required(ErrorMessage = "Enter the entry fee."), Display(Name = "Entry fee (R)")]
    public decimal? EntryFee { get; set; } = 0;

    public object ToApiBody() => new { Name, DistanceKm, MaxParticipants, EntryFee };
}

public class CategoriesViewModel
{
    public required EventDto Event { get; init; }
    public CategoryFormViewModel NewCategory { get; init; } = new();
}

public class ResultFormViewModel
{
    public int EnrolmentId { get; set; }
    public int? ResultId { get; set; }
    public int EventId { get; set; }
    public TimeSpan? FinishTime { get; set; }
    public int? OverallPosition { get; set; }
    public int? CategoryPosition { get; set; }
    public string Status { get; set; } = "Finished";

    public object ToApiBody() => new { FinishTime, OverallPosition, CategoryPosition, Status };
}

public class EventEnrolmentsViewModel
{
    public required EventDto Event { get; init; }
    public required List<EnrolmentDto> Enrolments { get; init; }
    public bool HasFinished => Event.EventDate <= DateTime.Now;
}

public class HomeViewModel
{
    public List<EventDto> Events { get; init; } = [];
    public string? Discipline { get; init; }
    public string? Location { get; init; }
}

public class ProfileFormViewModel
{
    [Required(ErrorMessage = "Enter your full name."), Display(Name = "Full name")]
    public string FullName { get; set; } = "";
    [Required(ErrorMessage = "Enter your email address."), EmailAddress(ErrorMessage = "Enter a valid email address.")]
    public string Email { get; set; } = "";
    public string Role { get; set; } = "";
    public string? ProfilePictureUrl { get; set; }
}

public class ErrorViewModel
{
    public int? Code { get; init; }
}
