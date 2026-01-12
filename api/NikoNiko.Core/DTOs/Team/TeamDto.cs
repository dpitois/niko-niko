namespace NikoNiko.Core.DTOs.Team;

/// <summary>
/// Représente les données d'une équipe pour l'affichage.
/// </summary>
public class TeamDto
{
    /// <summary>
    /// L'identifiant unique de l'équipe.
    /// </summary>
    public Guid Id { get; set; }
    /// <summary>
    /// Le nom de l'équipe.
    /// </summary>
    public string Name { get; set; } = null!;
    /// <summary>
    /// L'identifiant de l'administrateur de l'équipe.
    /// </summary>
    public Guid AdminId { get; set; }
    /// <summary>
    /// Le nom de l'administrateur de l'équipe.
    /// </summary>
    public string AdminName { get; set; } = null!;
    /// <summary>
    /// La date de création de l'équipe.
    /// </summary>
    public DateTime CreatedAt { get; set; }
}