using HiLoGame.Application.Models;
namespace HiLoGame.Application.Abstractions
{
    public interface IGameService
    {
        Task<GameRoom> CreateRoomAsync(string name, int low, int high, int?
        secret, CancellationToken ct = default);
        Task<GameRoom> GetRoomAsync(string roomId, CancellationToken ct =
        default);
        Task<GameRoom> JoinRoomAsync(string roomId, string playerId, string
        playerName, CancellationToken ct = default);
        Task<GameRoom> StartMatchAsync(string roomId, CancellationToken ct =
        default);
        Task<(GuessResult result, GameRoom room)> MakeGuessAsync(string roomId,
        string playerId, int value, CancellationToken ct = default);
        Task<GameRoom> VoteRematchAsync(string roomId, string playerId, bool
        want, CancellationToken ct = default);
    }
}
