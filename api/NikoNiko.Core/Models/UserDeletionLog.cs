using System.ComponentModel.DataAnnotations;

namespace NikoNiko.Core.Models;

/// <summary>
/// Audit log for account deletions to comply with legal record-keeping 
/// while respecting GDPR right to erasure.
/// </summary>
public class UserDeletionLog
{
    /// <summary>
    /// Unique identifier for the log entry.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// A salted/hashed version of the user's identity (e.g., OAuth ID or Email) 
    /// for collision-checking or legal audit without storing PII.
    /// </summary>
    [Required]
    public string HashedIdentity { get; set; } = string.Empty;

    /// <summary>
    /// The timestamp when the deletion occurred.
    /// </summary>
    public DateTime DeletedAt { get; set; } = DateTime.UtcNow;
}