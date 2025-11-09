using HiLoGame.Application.Abstractions;
using HiLoGame.Application.Models;
namespace HiLoGame.Application.Services
{
    public sealed class RoomService : IRoomService
    {
        private readonly IStateStore _state;
        private const string Prefix = "room:";
        public RoomService(IStateStore state) => _state = state;
        public string BuildRoomKey(string roomName) => $"{Prefix}{roomName}";
        public Task SaveAsync(GameRoom room, CancellationToken ct = default) =>
        _state.SetAsync(BuildRoomKey(room.Name), room,
        TimeSpan.FromHours(6), ct);
        public Task<GameRoom?> TryGetAsync(string roomName, CancellationToken ct
        = default) =>
        _state.GetAsync<GameRoom>(BuildRoomKey(roomName), ct);
        public Task DeleteAsync(string roomName, CancellationToken ct = default)
        =>
        _state.RemoveAsync(BuildRoomKey(roomName), ct);
    }
}
