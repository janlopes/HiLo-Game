namespace HiLoGame.Api.Dtos
{
    public sealed record CreateRoomRequest(string Name, int? Low, int? High,
    int? Mystery);
    public sealed record CreateRoomResponse(string RoomId, string Name, int
    Low, int High);
    public sealed record JoinRoomRequest(string PlayerId, string PlayerName);
}
