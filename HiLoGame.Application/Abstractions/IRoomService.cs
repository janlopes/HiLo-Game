using HiLoGame.Application.Models;
namespace HiLoGame.Application.Abstractions
{
    public interface IRoomService
    {
        string BuildRoomKey(string roomId);
        Task SaveAsync(GameRoom room, CancellationToken ct = default);
        Task<GameRoom?> TryGetAsync(string roomId, CancellationToken ct =
        default);
        Task DeleteAsync(string roomId, CancellationToken ct = default);
    }
}