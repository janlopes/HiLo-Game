using HiLoGame.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
namespace HiLoGame.Infrastructure.Persistence
{

    public interface IMatchRepository
    {
        Task AddAsync(Match match, CancellationToken ct = default);
        Task<Match?> FindAsync(Guid id, CancellationToken ct = default);
    }
    public sealed class MatchRepository : IMatchRepository
    {
        private readonly GameDbContext _db;
        public MatchRepository(GameDbContext db) => _db = db;
        public async Task AddAsync(Match match, CancellationToken ct = default)
        {
            _db.Matches.Add(match);
            await _db.SaveChangesAsync(ct);
        }
        public Task<Match?> FindAsync(Guid id, CancellationToken ct = default)
        => _db.Matches.FirstOrDefaultAsync(x => x.Id == id, ct);
    }
}
