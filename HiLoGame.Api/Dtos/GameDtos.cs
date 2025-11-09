namespace HiLoGame.Api.Dtos
{
    public sealed record StartMatchResponse(string RoomId, string
    CurrentPlayerId);
    public sealed record GuessRequest(string PlayerId, int Value);
    public sealed record GuessResponse(string RoomId, string Hint, string?
    NextPlayerId);
    public sealed record StatusResponse(string RoomId, string Status, string?
    CurrentPlayerId, int Low, int High, int GuessCount);
    public sealed record VoteRematchRequest(string PlayerId, bool Want);
}
