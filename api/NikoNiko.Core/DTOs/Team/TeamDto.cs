namespace NikoNiko.Core.DTOs.Team;

/// <summary>
/// Represents the data of a team for display.
/// </summary>
public record TeamDto
{
    /// <summary>
    /// The unique identifier of the team.
    /// </summary>
    public Guid Id { get; init; }
    /// <summary>
    /// The name of the team.
    /// </summary>
    public string Name { get; init; } = null!;
    /// <summary>
    /// The ID of the team's administrator.
    /// </summary>
    public Guid AdminId { get; init; }
    /// <summary>
    /// The name of the team's administrator.
    /// </summary>
    public string? AdminName { get; init; }
    /// <summary>
    /// The date and time when the team was created.
    /// </summary>
    public DateTime CreatedAt { get; init; }
}