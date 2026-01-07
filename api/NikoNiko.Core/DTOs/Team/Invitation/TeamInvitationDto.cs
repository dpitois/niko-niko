namespace NikoNiko.Core.DTOs.Team.Invitation
{
    public class TeamInvitationDto
    {
        public Guid Id { get; set; }
        public Guid TeamId { get; set; }
        public string? TeamName { get; set; } // Made nullable
        public Guid CreatorUserId { get; set; }
        public string? CreatorUserName { get; set; } // Made nullable
        public string Token { get; set; } = null!;
        public DateTime ExpirationDate { get; set; }
        public string Status { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public Guid? AcceptedByUserId { get; set; }
        public DateTime? AcceptedAt { get; set; }
    }
}