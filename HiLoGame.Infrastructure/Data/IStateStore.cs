namespace HiLoGame.Application.Abstractions
{
    public interface IStateStore
    {
        Task<T?> GetAsync<T>(string key, CancellationToken ct = default);
        Task SetAsync<T>(string key, T value, TimeSpan? ttl = null, CancellationToken ct = default);
        Task RemoveAsync(string key, CancellationToken ct = default);
    }
}