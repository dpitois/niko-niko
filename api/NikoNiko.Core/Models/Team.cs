using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NikoNiko.Core.Models;

/// <summary>
/// Represents an Agile team.
/// </summary>
public class Team
{
    /// <summary>
    /// Unique identifier for the team.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The name of the team.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = null!;

    /// <summary>
    /// The ID of the user who manages the team.
    /// </summary>
    [Required]
    public Guid AdminId { get; set; }

    /// <summary>
    /// Navigation property for the team's administrator.
    /// </summary>
    [ForeignKey("AdminId")]
    public User Admin { get; set; } = null!;

    /// <summary>
    /// The date and time when the team was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Navigation property for the members of the team.
    /// </summary>
    public List<TeamUser> TeamUsers { get; set; } = new();

    /// <summary>
    /// Navigation property for the sprints associated with this team.
    /// </summary>
    public List<Sprint> Sprints { get; set; } = new();
}