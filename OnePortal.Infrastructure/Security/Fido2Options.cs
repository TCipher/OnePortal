using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnePortal.Infrastructure.Security
{
    public class Fido2Options
    {
        public string RpId { get; set; } = default!;          // e.g., auth.oneportal.local (must be registrable domain)
        public string RpName { get; set; } = "OnePortal";
        public string[] Origins { get; set; } = Array.Empty<string>(); // e.g., https://app.oneportal.local
    }
}
