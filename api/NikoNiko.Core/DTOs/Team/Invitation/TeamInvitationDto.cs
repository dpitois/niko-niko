namespace NikoNiko.Core.DTOs.Team.Invitation
{
    /// <summary>
    /// Represents the data of a team invitation for display.
    /// </summary>
    public record TeamInvitationDto
    {
        /// <summary>
        /// Unique identifier for the invitation.
        /// </summary>
        public Guid Id { get; init; }
        /// <summary>
        /// The ID of the team the invitation is for.
        /// </summary>
        public Guid TeamId { get; init; }
        /// <summary>
        /// The name of the team the invitation is for.
        /// </summary>
        public string? TeamName { get; init; }
        /// <summary>
        /// The ID of the user who created the invitation.
        /// </summary>
        public Guid CreatorUserId { get; init; }
        /// <summary>
        /// The name of the user who created the invitation.
        /// </summary>
        public string? CreatorUserName { get; init; }
        /// <summary>
        /// Unique token used to identify and accept the invitation.
        /// </summary>
        public string Token { get; init; } = null!;
        /// <summary>
        /// The date and time when the invitation expires.
        /// </summary>
        public DateTime ExpirationDate { get; init; }
        /// <summary>
        /// The current status of the invitation (e.g., Pending, Accepted).
        /// </summary>
        public string Status { get; init; } = null!;
        /// <summary>
        /// The date and time when the invitation was created.
        /// </summary>
        public DateTime CreatedAt { get; init; }
        /// <summary>
        /// The ID of the user who accepted the invitation, if any.
        /// </summary>
        public Guid? AcceptedByUserId { get; init; }
        /// <summary>
        /// The date and time when the invitation was accepted.
        /// </summary>
        public DateTime? AcceptedAt { get; init; }
    }
}