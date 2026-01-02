namespace NikoNiko.Core.DTOs.User;

/// <summary>
/// Represents the data of a user for display.
/// </summary>
public class UserDto
{
    /// <summary>
    /// The unique identifier of the user.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The email address of the user.
    /// </summary>
    public string Email { get; set; } = null!;

    /// <summary>
    /// The full name of the user.
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// The URL of the user's avatar (optional).
    /// </summary>
    public string? AvatarUrl { get; set; }

    /// <summary>
    /// The date the user account was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }
}