using System.ComponentModel.DataAnnotations;

using System.ComponentModel.DataAnnotations.Schema;



namespace RaceDay.Api.Models

{

    public class Enrolment

    {

        [Key]

        public int EnrolmentId { get; set; }



        [Required]

        public int ParticipantId { get; set; }



        [ForeignKey("ParticipantId")]

        public User? Participant { get; set; }



        [Required]

        public int CategoryId { get; set; }



        [ForeignKey("CategoryId")]

        public Category? Category { get; set; }



        public DateTime EnrolmentDate { get; set; } = DateTime.Now;



        [MaxLength(20)]

        public string Status { get; set; } = "Confirmed";



        public Result? Result { get; set; }

    }

}




