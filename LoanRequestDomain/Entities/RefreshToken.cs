using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanRequestDomain.Entities
{
    public class RefreshToken
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        // Foreign Key to your Identity User
        public string? UserId { get; set; }
        public string Token { get; set; } = string.Empty; // Store as Hash
        public DateTime ExpiresAt { get; set; }
        public DateTime? RevokedAt { get; set; }

        public string? DeviceInfo { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Helper: Is this token currently valid?
       // public bool IsActive => RevokedAt == null && DateTime.UtcNow < ExpiresAt;
    }
}
