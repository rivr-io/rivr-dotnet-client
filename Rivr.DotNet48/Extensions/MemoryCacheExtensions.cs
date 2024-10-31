using System;
using System.Runtime.Caching;
using System.Threading.Tasks;
using Rivr.DotNet48.Models.Authentication;

namespace Rivr.DotNet48.Extensions;

/// <summary>
/// Extension methods for <see cref="MemoryCache"/>.
/// </summary>
public static class MemoryCacheExtensions
{
    /// <summary>
    /// Gets an item from the cache if it exists, otherwise creates it using the specified factory.
    /// </summary>
    /// <param name="memoryCache"></param>
    /// <param name="key"></param>
    /// <param name="factory"></param>
    /// <returns></returns>
    public static async Task<TokenResponse> GetOrCreateAsync(this MemoryCache memoryCache, string key, Func<Task<TokenResponse>> factory)
    {
        if (memoryCache.Contains(key)) return memoryCache.Get(key) as TokenResponse;
        if (await factory() is { } result)
        {
            memoryCache.Set(key, result, new CacheItemPolicy
            {
                SlidingExpiration = TimeSpan.FromSeconds(result.ExpiresIn)
            });
        }

        return memoryCache.Get(key) as TokenResponse;
    }
}