using HiLoGame.Api.Dtos;
using HiLoGame.Application.Abstractions;
using Microsoft.AspNetCore.Mvc;
namespace HiLoGame.Api.Controllers
{
    [ApiController]
    [Route("api/rooms")]
    public sealed class RoomsController : ControllerBase
    {
        private readonly IGameService _game;

        public RoomsController(IGameService game)
        {
            _game = game;
        }

        [HttpPost("create")]
        public async Task<ActionResult<CreateRoomResponse>> Create([FromBody] CreateRoomRequest req, CancellationToken ct)
        {
            int low = req.Low ?? 1;
            int high = req.High ?? 100;
            var room = await _game.CreateRoomAsync(req.Name, low, high,
            req.Mystery, ct);
            return Ok(new CreateRoomResponse(room.RoomId, room.Name, room.Low,
            room.High));
        }

        [HttpGet("{roomId}")]
        public async Task<IActionResult> Get(string roomId, CancellationToken ct)
        {
            var room = await _game.GetRoomAsync(roomId, ct);
            room.Mystery = 0;
            return Ok(room);
        }

        [HttpPost("{roomId}/join")]
        public async Task<IActionResult> Join(string roomId, [FromBody] JoinRoomRequest req, CancellationToken ct)
        {
            var room = await _game.JoinRoomAsync(roomId, req.PlayerId,
            req.PlayerName, ct);
            return Ok(room);
        }
    }
}
