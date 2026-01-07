using System.ComponentModel.DataAnnotations;

namespace NikoNiko.Core.DTOs.Team;

/// <summary>
/// Représente les données nécessaires pour créer une nouvelle équipe.
/// </summary>
public class CreateTeamDto
{
    /// <summary>
    /// Le nom de l'équipe.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = null!;
}