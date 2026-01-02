namespace NikoNiko.Core.DTOs.Team.Invitation
{
    public class CreateTeamInvitationDto
    {
        public Guid TeamId { get; set; }
        public int ExpirationInDays { get; set; } = 7; // Default to 7 days
    }
}
