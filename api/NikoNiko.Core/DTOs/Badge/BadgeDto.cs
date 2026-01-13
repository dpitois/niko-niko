namespace NikoNiko.Core.DTOs.Badge;

/// <summary>
/// Represents the data of a badge for display.
/// </summary>
public record BadgeDto
{
    /// <summary>
    /// The unique identifier of the badge.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// The ID of the user who earned the badge.
    /// </summary>
    public Guid UserId { get; init; }

    /// <summary>
    /// The name of the badge.
    /// </summary>
    public string Name { get; init; } = null!;

    /// <summary>
    /// The description of the badge.
    /// </summary>
    public string Description { get; init; } = null!;

    /// <summary>
    /// The date the badge was earned.
    /// </summary>
    public DateTime EarnedAt { get; init; }
}