namespace HiLoGame.Application.Models
{
    public sealed record TurnChangedEvent(string RoomId, string
    CurrentPlayerId);
    public sealed record GuessMadeEvent(string RoomId, Guess Guess);
    public sealed record MatchEndedEvent(string RoomId, string WinnerPlayerId);
}
