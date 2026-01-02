using System;

namespace NikoNiko.Core.Models
{
    public class TeamInvitation
    {
        public Guid Id { get; set; }
        public Guid TeamId { get; set; }
        public Team Team { get; set; }
        public Guid CreatorUserId { get; set; }
        public User CreatorUser { get; set; }
        public string Token { get; set; }
        public DateTime ExpirationDate { get; set; }
        public string Status { get; set; } = "Pending"; // Default status
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public Guid? AcceptedByUserId { get; set; }
        public User AcceptedByUser { get; set; }
        public DateTime? AcceptedAt { get; set; }
        public bool IsDeleted { get; set; } = false; // Pour le soft delete
    }
}
