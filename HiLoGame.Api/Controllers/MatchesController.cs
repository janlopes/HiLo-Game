using HiLoGame.Api.Dtos;
using HiLoGame.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace HiLoGame.Api.Controllers
{
    [ApiController]
    [Route("api/matches")]
    public sealed class MatchesController : ControllerBase
    {
        private readonly GameDbContext _db;
        public MatchesController(GameDbContext db) => _db = db;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MatchDto>>> List(CancellationToken ct)
        {
            var data = await _db.Matches
                .AsNoTracking()
                .Select(x => new MatchDto(x.Id, x.RoomId, x.RoomName, x.WinnerName, x.StartedAt, x.EndedAt))
                .ToListAsync(ct);
            return Ok(data.OrderByDescending(x => x.EndedAt));
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Get(Guid id, CancellationToken ct)
        {

            var match = await _db.Matches.Include(m =>
            m.Logs).FirstOrDefaultAsync(x => x.Id == id, ct);
            return match is null ? NotFound() : Ok(match);
        }
    }
}
