using System.ComponentModel.DataAnnotations;

namespace AutoEdge.Models.Entities;

public class MechanicUser
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(450)]
    public string UserId { get; set; } = string.Empty; // AspNetUsers.Id

    [Required]
    public int MechanicId { get; set; }
    public Mechanic Mechanic { get; set; } = null!;
}