using System;

namespace NikoNiko.Core.Models
{
    /// <summary>
    /// Represents an invitation to join a team.
    /// </summary>
    public class TeamInvitation
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
        /// Navigation property for the team associated with the invitation.
        /// </summary>
        public Team? Team { get; set; }

        /// <summary>
        /// The ID of the user who created the invitation.
        /// </summary>
        public Guid? CreatorUserId { get; set; }

        /// <summary>
        /// Navigation property for the user who created the invitation.
        /// </summary>
        public User? CreatorUser { get; set; }

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
        public string Status { get; set; } = "Pending";

        /// <summary>
        /// The date and time when the invitation was created.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// The ID of the user who accepted the invitation, if any.
        /// </summary>
        public Guid? AcceptedByUserId { get; set; }

        /// <summary>
        /// Navigation property for the user who accepted the invitation.
        /// </summary>
        public User? AcceptedByUser { get; set; }

        /// <summary>
        /// The date and time when the invitation was accepted.
        /// </summary>
        public DateTime? AcceptedAt { get; set; }

        /// <summary>
        /// Indicates if the invitation has been soft-deleted.
        /// </summary>
        public bool IsDeleted { get; set; } = false;
    }
}