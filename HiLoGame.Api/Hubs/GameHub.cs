using Microsoft.AspNetCore.SignalR;
namespace HiLoGame.Api.Hubs
{
    public sealed class GameHub : Hub
    {
        public Task JoinRoomGroup(string roomId) =>
        Groups.AddToGroupAsync(Context.ConnectionId, $"room:{roomId}");
    }
}
