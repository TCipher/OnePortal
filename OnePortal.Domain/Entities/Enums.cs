using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnePortal.Domain.Entities
{
    public enum MfaMethod : short
    {
        EmailOtp = 1,
        WebAuthn = 2
    }

    public enum Gender : short
    {
        Male = 1,
        Female = 2
    }

    public enum WorkLocation : short
    {
        Zone = 1,
        VI = 2
    }

    // Optional: if you plan to make Level an enum too (kept as short? in UserDetails for now)
    public enum Level : short
    {
        Management = 1,
        MidManagement = 2,
        Staff = 3
    }

    public enum SkillType : short
    {
        Skilled = 1,
        UnSkilled = 2,
        
    }

    public enum JobType : short
    {
        Permanent = 1,
        Casual = 2,
        Contract = 3,
        Internship = 4
    }
}
