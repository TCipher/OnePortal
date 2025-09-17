using OnePortal.Domain.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnePortal.Domain.Entities
{
    public class RefreshToken : BaseEntity
    {
        public int UserId { get; set; }
        public UserDetails User { get; set; } = default!;
        public string TokenHash { get; set; } = default!;
        public DateTime ExpiresUtc { get; set; }
        public DateTime? ConsumedUtc { get; set; }
        public string? ReplacedByTokenHash { get; set; }
        public string? DeviceInfo { get; set; } // UA fingerprint hint
        public string? IpAddress { get; set; }
    }
}
