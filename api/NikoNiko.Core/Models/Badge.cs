using System.ComponentModel.DataAnnotations;

namespace NikoNiko.Core.Models;

public class Badge
{
    public Guid Id { get; set; }

    [Required]
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = null!;

    [Required]
    public string Description { get; set; } = null!;

    public DateTime EarnedAt { get; set; } = DateTime.UtcNow;
}