using HiLoGame.Application.Abstractions;
using HiLoGame.Application.Models;
using HiLoGame.Infrastructure.Data;
using HiLoGame.Infrastructure.Persistence;
using Match = HiLoGame.Infrastructure.Data.Match;
namespace HiLoGame.Application.Services
{
    public sealed class MatchService : IMatchService
    {
        private readonly IMatchRepository _repo;
        public MatchService(IMatchRepository repo) => _repo = repo;
        public async Task PersistFinishedMatchAsync(GameRoom room,
        CancellationToken ct = default)
        {
            if (room.Status != Models.RoomStatus.Finished) return;
            var winnerGuess = room.Guesses.LastOrDefault(g => g.Result ==
            Models.GuessResult.Correct)
            ?? throw new InvalidOperationException("No winning guess found");
            var winner = room.Players.First(p => p.PlayerId ==
            winnerGuess.PlayerId);
            var match = new Match
            {
                Id = Guid.NewGuid(),
                RoomId = room.RoomId,
                RoomName = room.Name,
                Low = room.Low,
                High = room.High,
                Secret = room.Mystery,
                StartedAt = room.CreatedAt,
                EndedAt = winnerGuess.At,
                WinnerPlayerId = winner.PlayerId,
                WinnerName = winner.Name,
                Players = room.Players.Select(p => new PlayerSnapshot
                {
                    PlayerId = p.PlayerId,
                    Name = p.Name
                }).ToList(),
                Logs = room.Guesses.Select(g => new GuessLog
                {
                    Id = Guid.NewGuid(),
                    MatchId = Guid.Empty, // set by EF when added
                    PlayerId = g.PlayerId,
                    PlayerName = room.Players.First(p => p.PlayerId ==
                    g.PlayerId).Name,
                    Value = g.Value,
                    Result = g.Result.ToString(),
                    At = g.At
                }).ToList()
            };
            await _repo.AddAsync(match, ct);
        }
    }
}
