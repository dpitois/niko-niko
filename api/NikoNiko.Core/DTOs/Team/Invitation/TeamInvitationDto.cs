namespace NikoNiko.Core.DTOs.Team.Invitation
{
    public class TeamInvitationDto
    {
        public Guid Id { get; set; }
        public Guid TeamId { get; set; }
        public string TeamName { get; set; }
        public Guid CreatorUserId { get; set; }
        public string CreatorUserName { get; set; }
        public DateTime ExpirationDate { get; set; }
        public string Token { get; set; }
        public string Status { get; set; } // e.g., "Pending", "Accepted", "Expired"
    }
}
