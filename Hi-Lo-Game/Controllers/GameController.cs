using HiLoGame.Api.Dtos;
using HiLoGame.Api.Hubs;
using HiLoGame.Application.Abstractions;
using HiLoGame.Application.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
namespace HiLoGame.Api.Controllers
{
    [ApiController]
    [Route("api/game")]
    public sealed class GameController : ControllerBase
    {
        private readonly IGameService _game;
        private readonly IHubContext<GameHub> _hub;
        public GameController(IGameService game, IHubContext<GameHub> hub)
        {
            _game = game; _hub = hub;
        }

        [HttpPost("{roomId}/start")]
        public async Task<ActionResult<StartMatchResponse>> Start(string roomId, CancellationToken ct)
        {
            var room = await _game.StartMatchAsync(roomId, ct);
            await _hub.Clients.Group($"room:{roomId}").SendAsync("turnChanged",
            room.CurrentPlayer?.PlayerId, ct);
            return Ok(new StartMatchResponse(room.RoomId,
            room.CurrentPlayer!.PlayerId));
        }

        [HttpPost("{roomId}/guess")]
        public async Task<ActionResult<GuessResponse>> Guess(string roomId, [FromBody] GuessRequest req, CancellationToken ct)
        {
            var (result, room) = await _game.MakeGuessAsync(roomId,
            req.PlayerId, req.Value, ct);
            string hint = result switch
            {
                GuessResult.Correct => "CORRECT",
                GuessResult.TooLow => "HI", // mystery number is higher than guess
                GuessResult.TooHigh => "LO", // mystery number is lower than guess
                _ => ""
            };
            if (result == GuessResult.Correct)
            {
                await _hub.Clients.Group($"room: {roomId}").SendAsync("matchEnded", req.PlayerId, ct);
                return Ok(new GuessResponse(roomId, hint, null));
            }
            else
            {
                var nextId = room.CurrentPlayer?.PlayerId;
                await _hub.Clients.Group($"room:{roomId}").SendAsync("turnChanged", nextId, ct);
                return Ok(new GuessResponse(roomId, hint, nextId));
            }
        }

        [HttpGet("{roomId}/status")]
        public async Task<ActionResult<StatusResponse>> Status(string roomId, CancellationToken ct)
        {
            var room = await _game.GetRoomAsync(roomId, ct);
            return Ok(new StatusResponse(
                room.RoomId,
                room.Status.ToString(),
                room.CurrentPlayer?.PlayerId,
                room.Low,
                room.High,
                room.Guesses.Count));
        }

        [HttpPost("{roomId}/vote-rematch")]
        public async Task<IActionResult> VoteRematch(string roomId, [FromBody] VoteRematchRequest req, CancellationToken ct)
        {
            var room = await _game.VoteRematchAsync(roomId, req.PlayerId,
            req.Want, ct);
            await _hub.Clients.Group($"room:{roomId}").SendAsync("voteUpdated",
            room.Players.Select(p => new { p.PlayerId, p.WantsRematch }), ct);
            if (room.Status == RoomStatus.InProgress && room.Guesses.Count == 0)
            {
                await _hub.Clients.Group($"room:{ roomId}").SendAsync("rematchStarted", ct);
            }
            return Ok(room);
        }
    }
}
