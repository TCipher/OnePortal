using Microsoft.Extensions.Caching.Distributed;
using OnePortal.Application.Abstractions;

namespace OnePortal.Infrastructure.Security;

public class ChallengeStore : IChallengeStore
{
    private readonly IDistributedCache _cache;
    public ChallengeStore(IDistributedCache cache) => _cache = cache;

    public async Task StoreAsync(string key, string json, TimeSpan ttl, CancellationToken ct)
        => await _cache.SetStringAsync(key, json, new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = ttl }, ct);

    public async Task<string?> TakeAsync(string key, CancellationToken ct)
    {
        var val = await _cache.GetStringAsync(key, ct);
        if (val is not null) await _cache.RemoveAsync(key, ct);
        return val;
    }
}
