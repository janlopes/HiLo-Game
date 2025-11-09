using System.Text.Json;
namespace HiLoGame.Application.Models
{
    public sealed class GameRoom
    {
        public required string RoomId { get; set; }
        public required string Name { get; set; }
        public int Low { get; set; }
        public int High { get; set; }
        public int Mystery { get; set; }
        public RoomStatus Status { get; set; } = RoomStatus.Lobby;
        public List<Player> Players { get; set; } = new();
        public int CurrentPlayerIndex { get; set; } = 0;
        public List<Guess> Guesses { get; set; } = new();
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public Player? CurrentPlayer => Players.Count == 0 ? null : Players[CurrentPlayerIndex % Players.Count];
        public void AddPlayer(Player p)
        {
            if (Players.Any(x => x.PlayerId == p.PlayerId)) return;
            Players.Add(p);
        }
        public void Start()
        {
            if (Players.Count < 1)
                throw new InvalidOperationException("Need at least 1 player to start.");
            Status = RoomStatus.InProgress;
            CurrentPlayerIndex = 0;
        }
        public GuessResult ApplyGuess(string playerId, int value)
        {
            if (Status != RoomStatus.InProgress) throw new
            InvalidOperationException("Match not in progress.");
            if (CurrentPlayer?.PlayerId != playerId) throw new
            InvalidOperationException("Not your turn.");
            if (value < Low || value > High) throw new
            ArgumentOutOfRangeException(nameof(value), $"Guess must be between {Low} and {High}.");
            GuessResult result = value == Mystery ? GuessResult.Correct : (value
            < Mystery ? GuessResult.TooLow : GuessResult.TooHigh);
            Guesses.Add(new Guess
            {
                PlayerId = playerId,
                Value = value,
                At =
            DateTimeOffset.UtcNow,
                Result = result
            });
            if (result == GuessResult.Correct)
            {
                Status = RoomStatus.Finished;
            }
            else
            {
                CurrentPlayerIndex = (CurrentPlayerIndex + 1) % Players.Count;
            }
            return result;
        }
        public void ResetForRematch(int? newSecret = null)
        {
            Guesses.Clear();
            foreach (var p in Players) p.WantsRematch = false;
            if (newSecret.HasValue) Mystery = newSecret.Value;

            Status = RoomStatus.InProgress;
            CurrentPlayerIndex = 0;
        }
        public string Serialize() => JsonSerializer.Serialize(this);
        public static GameRoom Deserialize(string json) =>
        JsonSerializer.Deserialize<GameRoom>(json)!;
    }
}
