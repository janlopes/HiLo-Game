using System.Text.Json;
using HiLoGame.Application.Abstractions;
using Microsoft.Extensions.Caching.Distributed;
namespace HiLoGame.Infrastructure.State

{
    public sealed class DistributedCacheStateStore : IStateStore
    {
        private static readonly JsonSerializerOptions _json =
        new(JsonSerializerDefaults.Web);
        private readonly IDistributedCache _cache;
        public DistributedCacheStateStore(IDistributedCache cache) => _cache =
        cache;
        public async Task<T?> GetAsync<T>(string key, CancellationToken ct =
        default)
        {
            var bytes = await _cache.GetAsync(key, ct);
            if (bytes is null) return default;
            return JsonSerializer.Deserialize<T>(bytes, _json);
        }
        public async Task SetAsync<T>(string key, T value, TimeSpan? ttl =
        null, CancellationToken ct = default)
        {
            var bytes = JsonSerializer.SerializeToUtf8Bytes(value, _json);
            var options = new DistributedCacheEntryOptions();
            if (ttl is not null) options.SetSlidingExpiration(ttl.Value);
            await _cache.SetAsync(key, bytes, options, ct);
        }
        public Task RemoveAsync(string key, CancellationToken ct = default) =>
        _cache.RemoveAsync(key, ct);
    }
}
