using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnePortal.Domain.Abstractions
{
    public interface ISoftDeletable
    {
        bool IsActive { get; set; }
    }
}
