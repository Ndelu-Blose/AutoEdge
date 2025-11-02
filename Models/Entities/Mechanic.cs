using System.ComponentModel.DataAnnotations;

namespace AutoEdge.Models.Entities
{
    public class Mechanic
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(80)]
        public string Name { get; set; } = string.Empty;

        // Stored as pipe-delimited string in DB via value converter
        public List<string> Skills { get; set; } = new();

        public double Rating { get; set; } = 0.0;
        public bool IsAvailable { get; set; } = true;

        public virtual ICollection<ServiceJob> ServiceJobs { get; set; } = new List<ServiceJob>();
    }
}