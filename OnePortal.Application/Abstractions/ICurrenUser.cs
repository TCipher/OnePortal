using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnePortal.Application.Abstractions
{
    public interface ICurrentUser
    {
        int? UserId { get; }
        bool IsInGlobalRole(string roleCode);
    }
}
