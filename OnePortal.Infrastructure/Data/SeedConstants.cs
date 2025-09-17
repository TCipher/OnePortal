using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnePortal.Infrastructure.Data
{


    public static class SeedConstants
    {
        // 15 default portals (codes must be unique)
        public static readonly Portal[] Portals = new[]
  {
    new Portal { Id=1,  Name="Sustainability",               Code="SIMS" },
    new Portal { Id=2,  Name="Exit Management",              Code="EMP" },
    new Portal { Id=3,  Name="Billing",                      Code="BILLING" },
    new Portal { Id=4,  Name="Regulatory",                   Code="REGULATORY" },
    new Portal { Id=5,  Name="E-Service",                    Code="E-SERVICE" },
    new Portal { Id=6,  Name="Procurement",                  Code="PROCURE" },
    new Portal { Id=7,  Name="Finance",                      Code="FIN" },
    new Portal { Id=8,  Name="Digi docs",                    Code="DIGI-DOCS" },
    new Portal { Id=9,  Name="Asset Mgt",                    Code="ASSET" },
    new Portal { Id=10, Name="Truck Park",                   Code="TP" },
    new Portal { Id=11, Name="Immigration",                  Code="IMG" },
    new Portal { Id=12, Name="Lease Tracking",               Code="LT" },
    new Portal { Id=13, Name="Visitor Mgt",                  Code="VM" },
    new Portal { Id=14, Name="Equipment Efficiency",         Code="EF" },
    new Portal { Id=15, Name="Security Excellence System",   Code="SES" },
    new Portal { Id=16, Name="Project Tracking",             Code="PT" },
    new Portal { Id=17, Name="Engagement Survey",            Code="ES" }
};

        // PortalRoles will stay unique now that Portal.Id is unique:
        public static readonly PortalRole[] PortalRoles = Portals.SelectMany(p => new[]
        {
    new PortalRole { Id = 1000 + p.Id*10 + 1, PortalId = p.Id, Name="Admin",    Code="ADMIN" },
    new PortalRole { Id = 1000 + p.Id*10 + 2, PortalId = p.Id, Name="Manager",  Code="MANAGER" },
    new PortalRole { Id = 1000 + p.Id*10 + 3, PortalId = p.Id, Name="Employee", Code="EMPLOYEE" },
}).ToArray();

    }
}
