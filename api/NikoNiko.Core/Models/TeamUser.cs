using System.ComponentModel.DataAnnotations;

namespace NikoNiko.Core.Models;

public class TeamUser
{
    [Required]
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    [Required]
    public Guid TeamId { get; set; }
    public Team Team { get; set; } = null!;

    public bool IsAdmin { get; set; } = false;

    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
}