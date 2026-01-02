using System.ComponentModel.DataAnnotations;

namespace NikoNiko.Core.Models;

public class MoodEntry
{
    public Guid Id { get; set; }

    [Required]
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    [Required]
    public Guid SprintId { get; set; }
    public Sprint Sprint { get; set; } = null!;

    public DateTime Date { get; set; } = DateTime.UtcNow;

    [Required]
    public MoodType Mood { get; set; }
}