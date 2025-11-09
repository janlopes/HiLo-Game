using HiLoGame.Application.Abstractions;
using HiLoGame.Application.Models;
using HiLoGame.Application.Services;
using System.Text.Json;

namespace HiLoGame.Tests.Application
{
    public class RoomServiceTests
    {
        private sealed class MemoryStateStore : IStateStore
        {
            private readonly Dictionary<string, byte[]> _bytes = new();
            private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);
            public Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
            {
                if (!_bytes.TryGetValue(key, out var data)) return Task.FromResult<T?>(default);
                var val = JsonSerializer.Deserialize<T>(data, JsonOpts);   // <-- specify options
                return Task.FromResult<T?>(val);
            }

            public Task SetAsync<T>(string key, T value, TimeSpan? ttl = null, CancellationToken ct = default)
            {
                var data = JsonSerializer.SerializeToUtf8Bytes(value, JsonOpts); // <-- specify options
                _bytes[key] = data;
                return Task.CompletedTask;
            }

            public Task RemoveAsync(string key, CancellationToken ct = default)
            {
                _bytes.Remove(key);
                return Task.CompletedTask;
            }
        }

        [Fact]
        public void BuildRoomKey_has_expected_prefix()
        {
            var store = new MemoryStateStore();
            var svc = new RoomService(store);

            var key = svc.BuildRoomKey("abc");
            Assert.Equal("room:abc", key);
        }

        [Fact]
        public async Task Save_TryGet_Delete_roundtrip()
        {
            var store = new MemoryStateStore();
            var svc = new RoomService(store);

            var room = new GameRoom { RoomId = "id1", Name = "R", Low = 1, High = 100, Mystery = 42 };

            await svc.SaveAsync(room);
            var loaded = await svc.TryGetAsync(room.Name);
            Assert.NotNull(loaded);
            Assert.Equal("R", loaded.Name);

            await svc.DeleteAsync(room.Name);
            var afterDelete = await svc.TryGetAsync(room.Name);
            Assert.Null(afterDelete);
        }
    }
}
