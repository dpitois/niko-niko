namespace NikoNiko.Core.DTOs.Team.Invitation
{
    /// <summary>
    /// Represents the data of a team invitation for display.
    /// </summary>
    public class TeamInvitationDto
    {
        /// <summary>
        /// Unique identifier for the invitation.
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// The ID of the team the invitation is for.
        /// </summary>
        public Guid TeamId { get; set; }
        /// <summary>
        /// The name of the team the invitation is for.
        /// </summary>
        public string? TeamName { get; set; }
        /// <summary>
        /// The ID of the user who created the invitation.
        /// </summary>
        public Guid CreatorUserId { get; set; }
        /// <summary>
        /// The name of the user who created the invitation.
        /// </summary>
        public string? CreatorUserName { get; set; }
        /// <summary>
        /// Unique token used to identify and accept the invitation.
        /// </summary>
        public string Token { get; set; } = null!;
        /// <summary>
        /// The date and time when the invitation expires.
        /// </summary>
        public DateTime ExpirationDate { get; set; }
        /// <summary>
        /// The current status of the invitation (e.g., Pending, Accepted).
        /// </summary>
        public string Status { get; set; } = null!;
        /// <summary>
        /// The date and time when the invitation was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }
        /// <summary>
        /// The ID of the user who accepted the invitation, if any.
        /// </summary>
        public Guid? AcceptedByUserId { get; set; }
        /// <summary>
        /// The date and time when the invitation was accepted.
        /// </summary>
        public DateTime? AcceptedAt { get; set; }
    }
}