using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

using System.ComponentModel.DataAnnotations.Schema;



namespace RaceDay.Api.Models

{

    public class Category

    {

        [Key]

        public int CategoryId { get; set; }



        [Required]

        public int EventId { get; set; }



        [ForeignKey("EventId")]

        public Event? Event { get; set; }



        [Required]

        [MaxLength(50)]

        public string Name { get; set; } = string.Empty;



        [Required]

        [Column(TypeName = "decimal(5,2)")]

        public decimal DistanceKm { get; set; }



        public int MaxParticipants { get; set; } = 100;



        [Column(TypeName = "decimal(8,2)")]

        public decimal EntryFee { get; set; } = 0;



        // Navigation properties

        public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();

    }

}