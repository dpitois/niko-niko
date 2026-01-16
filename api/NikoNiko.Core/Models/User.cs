using System.ComponentModel.DataAnnotations;

namespace NikoNiko.Core.Models;

/// <summary>
/// Represents a user within the NikoNiko application.
/// </summary>
public class User
{
    /// <summary>
    /// Unique identifier for the user.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The unique ID provided by the OAuth provider (e.g., GitHub, Google).
    /// </summary>
    [Required]
    public string OAuthId { get; set; } = null!;

    /// <summary>
    /// The user's email address.
    /// </summary>
    [EmailAddress]
    public string? Email { get; set; }

    /// <summary>
    /// The user's full name or display name.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = null!;

    /// <summary>
    /// URL to the user's avatar image.
    /// </summary>
    public string? AvatarUrl { get; set; }

    /// <summary>
    /// Indicates whether the user has system-wide administrative privileges.
    /// </summary>
    public bool IsSuperAdmin { get; set; } = false;

    /// <summary>
    /// The date and time when the user account was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Navigation property for the relationship between users and teams.
    /// </summary>
    public List<TeamUser> TeamUsers { get; set; } = new();

    /// <summary>
    /// Navigation property for the badges earned by the user.
    /// </summary>
    public List<Badge> Badges { get; set; } = new();
}