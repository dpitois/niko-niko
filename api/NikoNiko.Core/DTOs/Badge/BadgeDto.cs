namespace NikoNiko.Core.DTOs.Badge;

/// <summary>
/// Represents the data of a badge for display.
/// </summary>
public class BadgeDto
{
    /// <summary>
    /// The unique identifier of the badge.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The ID of the user who earned the badge.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// The name of the badge.
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// The description of the badge.
    /// </summary>
    public string Description { get; set; } = null!;

    /// <summary>
    /// The date the badge was earned.
    /// </summary>
    public DateTime EarnedAt { get; set; }
}