using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace NikoNiko.Core.Models;

public class RefreshToken
{
    [JsonIgnore]
    public int Id { get; set; }

    public string Token { get; set; } = null!;
    public DateTime Expires { get; set; }
    public DateTime Created { get; set; }
    public string? CreatedByIp { get; set; }
    public DateTime? Revoked { get; set; }
    public string? RevokedByIp { get; set; }
    public string? ReplacedByToken { get; set; }
    public string? ReasonRevoked { get; set; }
    public bool IsExpired => DateTime.UtcNow >= Expires;
    public bool IsRevoked => Revoked != null;
    public bool IsActive => !IsRevoked && !IsExpired;

    public Guid UserId { get; set; }
    [JsonIgnore]
    public User User { get; set; } = null!;
}