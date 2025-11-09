namespace HiLoGame.Application.Models
{
    public sealed class Player
    {
        public required string PlayerId { get; init; }
        public required string Name { get; init; }
        public bool WantsRematch { get; set; } = false;
    }
}