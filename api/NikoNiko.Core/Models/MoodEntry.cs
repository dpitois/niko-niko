using System.ComponentModel.DataAnnotations;

namespace NikoNiko.Core.Models;

/// <summary>
/// Records a user's mood for a specific date within a sprint.
/// </summary>
public class MoodEntry
{
    /// <summary>
    /// Unique identifier for the mood entry.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The ID of the user who recorded this mood.
    /// </summary>
    [Required]
    public Guid UserId { get; set; }

    /// <summary>
    /// Navigation property for the user who recorded this mood.
    /// </summary>
    public User User { get; set; } = null!;

    /// <summary>
    /// The ID of the sprint this mood entry belongs to.
    /// </summary>
    [Required]
    public Guid SprintId { get; set; }

    /// <summary>
    /// Navigation property for the sprint associated with this mood entry.
    /// </summary>
    public Sprint Sprint { get; set; } = null!;

    /// <summary>
    /// The date for which the mood is recorded.
    /// </summary>
    public DateTime Date { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// The type of mood recorded (e.g., Happy, Neutral, Sad).
    /// </summary>
    [Required]
    public MoodType Mood { get; set; }
}