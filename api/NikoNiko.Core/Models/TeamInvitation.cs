using System;

namespace NikoNiko.Core.Models
{
    public class TeamInvitation
    {
        public Guid Id { get; set; }
        public Guid TeamId { get; set; }
        public Team? Team { get; set; } // Made nullable
        public Guid CreatorUserId { get; set; }
        public User? CreatorUser { get; set; } // Made nullable
        public string Token { get; set; } = null!; // Keep as non-nullable, must be set
        public DateTime ExpirationDate { get; set; }
        public string Status { get; set; } = "Pending"; // Default status
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public Guid? AcceptedByUserId { get; set; }
        public User? AcceptedByUser { get; set; } // Made nullable
        public DateTime? AcceptedAt { get; set; }
        public bool IsDeleted { get; set; } = false; // Pour le soft delete
    }
}