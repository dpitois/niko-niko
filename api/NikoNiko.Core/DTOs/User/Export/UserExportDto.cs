namespace NikoNiko.Core.DTOs.User.Export;

public class UserExportDto
{
    public IdentityExportDto Identity { get; set; } = new();
    public List<TeamExportDto> Teams { get; set; } = new();
    public List<MoodExportDto> History { get; set; } = new();
}

public class IdentityExportDto
{
    public string OpenId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string? Provider { get; set; }
    public DateTime JoinedAt { get; set; }
}

public class TeamExportDto
{
    public string Id { get; set; } = string.Empty; // Normalized ID
    public string Name { get; set; } = string.Empty;
    public int MemberCount { get; set; }
}

public class MoodExportDto
{
    public DateTime Date { get; set; }
    public string Mood { get; set; } = string.Empty;
    public string TeamId { get; set; } = string.Empty; // Normalized ID reference
}