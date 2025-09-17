using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnePortal.Application.Abstractions
{
    public interface IChallengeStore
    {
        Task StoreAsync(string key, string json, TimeSpan ttl, CancellationToken ct);
        Task<string?> TakeAsync(string key, CancellationToken ct); // read & delete
    }
}
