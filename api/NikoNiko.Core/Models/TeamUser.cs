using System.ComponentModel.DataAnnotations;

namespace NikoNiko.Core.Models;

/// <summary>
/// Represents the join table for the many-to-many relationship between Users and Teams.
/// </summary>
public class TeamUser
{
    /// <summary>
    /// The ID of the user.
    /// </summary>
    [Required]
    public Guid UserId { get; set; }

    /// <summary>
    /// Navigation property for the user.
    /// </summary>
    public User User { get; set; } = null!;

    /// <summary>
    /// The ID of the team.
    /// </summary>
    [Required]
    public Guid TeamId { get; set; }

    /// <summary>
    /// Navigation property for the team.
    /// </summary>
    public Team Team { get; set; } = null!;

    /// <summary>
    /// Indicates whether the user has administrative rights within this specific team.
    /// </summary>
    public bool IsAdmin { get; set; } = false;

    /// <summary>
    /// The date and time when the user joined the team.
    /// </summary>
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
}