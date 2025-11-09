namespace HiLoGame.Api.Dtos
{
    public sealed record MatchDto(Guid Id, string RoomId, string RoomName,
    string WinnerName, DateTimeOffset StartedAt, DateTimeOffset EndedAt);
}
