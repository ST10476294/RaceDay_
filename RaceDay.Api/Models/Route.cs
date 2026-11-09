using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.Api.Models
{
    public class Route
    {
        [Key]
        public int RouteId { get; set; }

        [Required]
        public int EventId { get; set; }

        [ForeignKey("EventId")]
        public Event? Event { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public int? ElevationGainM { get; set; }

        [MaxLength(255)]
        public string? MapUrl { get; set; }
    }
}
