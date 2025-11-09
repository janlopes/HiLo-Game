using Microsoft.EntityFrameworkCore;
namespace HiLoGame.Infrastructure.Data
{
    public sealed class GameDbContext : DbContext
    {
        public GameDbContext(DbContextOptions<GameDbContext> options) :
        base(options)
        { }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Match>(b =>
            {
                b.HasKey(x => x.Id);

                b.Property(x => x.RoomId).HasMaxLength(64).IsRequired();
                b.Property(x => x.RoomName).HasMaxLength(128).IsRequired();

                // GuessLogs one-to-many
                b.HasMany(x => x.Logs)
                 .WithOne()
                 .HasForeignKey(x => x.MatchId)
                 .OnDelete(DeleteBehavior.Cascade);

                // PlayerSnapshot owned collection
                b.OwnsMany(x => x.Players, ps =>
                {
                    ps.WithOwner().HasForeignKey("MatchId");
                    ps.Property<int>("Id");
                    ps.HasKey("Id");
                    ps.Property(p => p.PlayerId).HasMaxLength(64).IsRequired();
                    ps.Property(p => p.Name).HasMaxLength(128).IsRequired();
                });
            });

            modelBuilder.Entity<GuessLog>(b =>
            {
                b.HasKey(g => g.Id);
                b.Property(g => g.PlayerId).HasMaxLength(64).IsRequired();
                b.Property(g => g.PlayerName).HasMaxLength(128).IsRequired();
                b.HasOne<Match>()
                 .WithMany(m => m.Logs)
                 .HasForeignKey(g => g.MatchId)
                 .OnDelete(DeleteBehavior.Cascade);
            });
        }

        public DbSet<Match> Matches => Set<Match>();
        public DbSet<GuessLog> GuessLogs => Set<GuessLog>();
    }
    public sealed class Match
    {
        public Guid Id { get; set; }
        public required string RoomId { get; set; }
        public required string RoomName { get; set; }
        public required int Low { get; set; }
        public required int High { get; set; }
        public required int Secret { get; set; }
        public required DateTimeOffset StartedAt { get; set; }
        public required DateTimeOffset EndedAt { get; set; }
        public required string WinnerPlayerId { get; set; }
        public required string WinnerName { get; set; }
        public List<GuessLog> Logs { get; set; } = new();
        public List<PlayerSnapshot> Players { get; set; } = new();
    }
    public sealed class PlayerSnapshot
    {
        public int Id { get; set; }
        public required string PlayerId { get; set; }
        public required string Name { get; set; }
    }
    public sealed class GuessLog
    {
        public Guid Id { get; set; }
        public required Guid MatchId { get; set; }
        public required string PlayerId { get; set; }
        public required string PlayerName { get; set; }
        public required int Value { get; set; }
        public required string Result { get; set; }
        public required DateTimeOffset At { get; set; }
    }
}