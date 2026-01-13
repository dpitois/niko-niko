using System.ComponentModel.DataAnnotations;

namespace NikoNiko.Core.DTOs.Team;

/// <summary>
/// Represents the data needed to create a new team.
/// </summary>
public record CreateTeamDto
{
    /// <summary>
    /// The name of the team.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Name { get; init; } = null!;
}