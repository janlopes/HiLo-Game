using HiLoGame.Application.Abstractions;
using HiLoGame.Application.Models;
namespace HiLoGame.Application.Services
{
    public sealed class GameService : IGameService
    {
        private readonly IRoomService _rooms;
        private readonly IMatchService _matches;
        public GameService(IRoomService rooms, IMatchService matches)
        {
            _rooms = rooms; _matches = matches;
        }
        public async Task<GameRoom> CreateRoomAsync(string name, int low, int high, int? secret, CancellationToken ct = default)
        {
            if (low >= high) throw new ArgumentException("low must be < high");
            var rng = new Random();
            var room = new GameRoom
            {
                RoomId = name,
                Name = name,
                Low = low,
                High = high,
                Mystery = secret ?? rng.Next(low, high + 1),
            };
            await _rooms.SaveAsync(room, ct);
            return room;
        }
        public async Task<GameRoom> GetRoomAsync(string roomName, CancellationToken ct = default)
        => await _rooms.TryGetAsync(roomName, ct) ?? throw new KeyNotFoundException("Room not found");
        public async Task<GameRoom> JoinRoomAsync(string roomName, string
        playerId, string playerName, CancellationToken ct = default)
        {
            var room = await GetRoomAsync(roomName, ct);
            if (room.Status != RoomStatus.Lobby)
                throw new InvalidOperationException("Cannot join; match already started.");

            room.AddPlayer(new Player
            {
                PlayerId = playerId,
                Name = playerName
            });
            await _rooms.SaveAsync(room, ct);
            return room;
        }
        public async Task<GameRoom> StartMatchAsync(string roomId, CancellationToken ct = default)
        {

            var room = await GetRoomAsync(roomId, ct);
            room.Start();
            await _rooms.SaveAsync(room, ct);
            return room;
        }
        public async Task<(GuessResult result, GameRoom room)> MakeGuessAsync(string roomId, string playerId, int value, CancellationToken ct = default)
        {
            var room = await GetRoomAsync(roomId, ct);
            var result = room.ApplyGuess(playerId, value);
            await _rooms.SaveAsync(room, ct);
            if (result == GuessResult.Correct)
            {
                await _matches.PersistFinishedMatchAsync(room, ct);
            }
            return (result, room);
        }

        public async Task<GameRoom> VoteRematchAsync(string roomId, string playerId, bool want, CancellationToken ct = default)
        {
            var room = await GetRoomAsync(roomId, ct);
            var player = room.Players.FirstOrDefault(p => p.PlayerId ==
            playerId)
            ?? throw new InvalidOperationException("Player not in room");

            player.WantsRematch = want;
            var yesCount = room.Players.Count(p => p.WantsRematch);
            if (room.Status == RoomStatus.Finished && yesCount >= 2)
            {
                var rng = new Random();
                room.ResetForRematch(rng.Next(room.Low, room.High + 1));
            }
            await _rooms.SaveAsync(room, ct);
            return room;
        }
    }
}