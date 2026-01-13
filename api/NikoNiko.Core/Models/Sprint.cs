using System.ComponentModel.DataAnnotations;

namespace NikoNiko.Core.Models;

/// <summary>
/// Represents a time-boxed work period (Sprint) for a team.
/// </summary>
public class Sprint
{
    /// <summary>
    /// Unique identifier for the sprint.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The name of the sprint (e.g., "Sprint 1", "January Iteration").
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = null!;

    /// <summary>
    /// The starting date and time of the sprint.
    /// </summary>
    [Required]
    public DateTime StartDate { get; set; }

    /// <summary>
    /// The ending date and time of the sprint.
    /// </summary>
    [Required]
    public DateTime EndDate { get; set; }

    /// <summary>
    /// The ID of the team this sprint belongs to.
    /// </summary>
    [Required]
    public Guid TeamId { get; set; }

    /// <summary>
    /// Navigation property for the team associated with this sprint.
    /// </summary>
    public Team Team { get; set; } = null!;

    /// <summary>
    /// Navigation property for the mood entries recorded during this sprint.
    /// </summary>
    public List<MoodEntry> MoodEntries { get; set; } = new();
}