namespace NikoNiko.Core.DTOs.Team.Invitation
{
    /// <summary>
    /// Represents the data needed to create a new team invitation.
    /// </summary>
    public class CreateTeamInvitationDto
    {
        /// <summary>
        /// The ID of the team for which the invitation is being created.
        /// </summary>
        public Guid TeamId { get; set; }
        /// <summary>
        /// The number of days before the invitation expires. Defaults to 7 days.
        /// </summary>
        public int ExpirationInDays { get; set; } = 7;
    }
}