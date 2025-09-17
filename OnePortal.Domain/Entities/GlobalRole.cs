using OnePortal.Domain.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnePortal.Domain.Entities
{
    public class GlobalRole : BaseEntity
    {
        public string Name { get; set; } = default!;
        public string Code { get; set; } = default!;
        public ICollection<UserDetails> Users { get; set; } = new List<UserDetails>();
    }

    public static class GlobalRoleCodes
    {
        public const string SuperAdmin = "SUPERADMIN";
        public const string Admin = "ADMIN";
        public const string User = "USER";
    }
}
