namespace NikoNiko.Core.DTOs.User;

/// <summary>
/// Represents the data of a user for display.
/// </summary>
public record UserDto
{
    /// <summary>
    /// The unique identifier of the user.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// The email address of the user.
    /// </summary>
    public string Email { get; init; } = null!;

    /// <summary>
    /// The full name of the user.
    /// </summary>
    public string Name { get; init; } = null!;

    /// <summary>
    /// The URL of the user's avatar (optional).
    /// </summary>
    public string? AvatarUrl { get; init; }

    /// <summary>
    /// The date the user account was created.
    /// </summary>
    public DateTime CreatedAt { get; init; }
}