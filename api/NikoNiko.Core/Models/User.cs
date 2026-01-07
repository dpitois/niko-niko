using System.ComponentModel.DataAnnotations;

namespace NikoNiko.Core.Models;

public class User
{
    public Guid Id { get; set; }

    [Required]
    public string OAuthId { get; set; } = null!;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = null!;

    public string? AvatarUrl { get; set; }

    public bool IsSuperAdmin { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<TeamUser> TeamUsers { get; set; } = new();

    public List<Badge> Badges { get; set; } = new();
}