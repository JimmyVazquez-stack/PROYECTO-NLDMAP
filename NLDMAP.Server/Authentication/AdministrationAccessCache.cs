using Microsoft.Extensions.Caching.Memory;
using NLDMAP.Application.Administration;

namespace NLDMAP.Server.Authentication;

public sealed class AdministrationAccessCache(
    IMemoryCache cache,
    IAdministrationStore store)
{
    private static readonly TimeSpan CacheDuration =
        TimeSpan.FromMinutes(1);

    public async Task<AdminAccess> GetAsync(ActorSession actor)
    {
        var cacheKey =
            $"administration-access:{actor.UserId}:{actor.SecurityStamp}";

        if (cache.TryGetValue(
                cacheKey,
                out AdminAccess? cachedAccess)
            && cachedAccess is not null)
        {
            return cachedAccess;
        }

        var access = await store.GetAccessAsync(actor);

        cache.Set(
            cacheKey,
            access,
            CacheDuration);

        return access;
    }

    public void Remove(ActorSession actor)
    {
        var cacheKey =
            $"administration-access:{actor.UserId}:{actor.SecurityStamp}";

        cache.Remove(cacheKey);
    }
}