using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NikoNiko.Core.Models;

public class Team
{
    public Guid Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = null!;

    [Required]
    public Guid AdminId { get; set; }

    [ForeignKey("AdminId")]
    public User Admin { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<TeamUser> TeamUsers { get; set; } = new();

    public List<Sprint> Sprints { get; set; } = new();
}