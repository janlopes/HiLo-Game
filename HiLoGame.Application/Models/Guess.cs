namespace HiLoGame.Application.Models
{
    public sealed class Guess
    {
        public required string PlayerId { get; init; }
        public required int Value { get; init; }
        public required DateTimeOffset At { get; init; }
        public required GuessResult Result { get; init; }
    }
}
