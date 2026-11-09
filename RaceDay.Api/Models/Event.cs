using System.ComponentModel.DataAnnotations;

using System.ComponentModel.DataAnnotations.Schema;



namespace RaceDay.Api.Models

{

    public class Event

    {

        [Key]

        public int EventId { get; set; }



        [Required]

        public int OrganiserId { get; set; }



        [ForeignKey("OrganiserId")]

        public User? Organiser { get; set; }



        [Required]

        [MaxLength(150)]

        public string Name { get; set; } = string.Empty;



        [Required]

        [MaxLength(20)]

        public string Discipline { get; set; } = string.Empty; // "Running", "Walking", "Cycling"



        [Required]

        public DateTime EventDate { get; set; }



        [Required]

        [MaxLength(150)]

        public string Location { get; set; } = string.Empty;



        [MaxLength(500)]

        public string? Description { get; set; }

        // Headline distance of the event, for example 42.2 for a marathon. Categories may offer other distances.
        [Column(TypeName = "decimal(5,2)")]
        public decimal DistanceKm { get; set; }

        // Public Azure Blob Storage address of the banner image.
        [MaxLength(500)]
        public string? BannerImageUrl { get; set; }



        // Navigation properties

        public ICollection<Category> Categories { get; set; } = new List<Category>();

        public ICollection<Route> Routes { get; set; } = new List<Route>();

    }

}