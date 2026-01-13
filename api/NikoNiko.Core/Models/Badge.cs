using System.ComponentModel.DataAnnotations;

namespace NikoNiko.Core.Models;

/// <summary>
/// Represents a gamification badge earned by a user.
/// </summary>
public class Badge
{
    /// <summary>
    /// Unique identifier for the badge instance.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The ID of the user who earned this badge.
    /// </summary>
    [Required]
    public Guid UserId { get; set; }

    /// <summary>
    /// Navigation property for the user who earned this badge.
    /// </summary>
    public User User { get; set; } = null!;

    /// <summary>
    /// The name of the badge.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = null!;

    /// <summary>
    /// A description of why the badge was awarded.
    /// </summary>
    [Required]
    public string Description { get; set; } = null!;

    /// <summary>
    /// The date and time when the badge was earned.
    /// </summary>
    public DateTime EarnedAt { get; set; } = DateTime.UtcNow;
}