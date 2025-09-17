using OnePortal.Domain.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnePortal.Domain.Entities
{
    public class PasswordResetToken : BaseEntity
    {
        public int UserId { get; set; }
        public UserDetails User { get; set; } = default!;
        public string TokenHash { get; set; } = default!;   // SHA256
        public DateTime ExpiresUtc { get; set; }
        public DateTime? UsedUtc { get; set; }
        public DateTime? CreatedUtc { get; set; } = DateTime.UtcNow;
    }
}
