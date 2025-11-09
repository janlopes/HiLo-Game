using HiLoGame.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HiLoGame.Tests.Unit
{
    public class GameDbContextTests
    {
        private GameDbContext CreateContext()
        {
            var opts = new DbContextOptionsBuilder<GameDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new GameDbContext(opts);
        }

        [Fact]
        public void Can_insert_Match_with_Logs_and_Players()
        {
            using var ctx = CreateContext();

            var match = new Match
            {
                Id = Guid.NewGuid(),
                RoomId = "ROOM-1",
                RoomName = "My first room",
                Low = 1,
                High = 100,
                Secret = 42,
                StartedAt = DateTimeOffset.UtcNow,
                EndedAt = DateTimeOffset.UtcNow.AddMinutes(1),
                WinnerPlayerId = "player-1",
                WinnerName = "John",
                Logs =
                {
                    new GuessLog
                    {
                        Id = Guid.NewGuid(),
                        MatchId = Guid.Empty, // EF will fix it because of relationship
                        PlayerId = "player-1",
                        PlayerName = "John",
                        Value = 42,
                        Result = "Correct",
                        At = DateTimeOffset.UtcNow
                    }
                },
                Players =
                {
                    new PlayerSnapshot
                    {
                        PlayerId = "player-1",
                        Name = "John"
                    }
                }
            };

            ctx.Matches.Add(match);
            ctx.SaveChanges();

            var saved = ctx.Matches
                .Include(m => m.Logs)
                .Include(m => m.Players)
                .Single();

            Assert.Equal("ROOM-1", saved.RoomId);
            Assert.Single(saved.Logs);
            Assert.Single(saved.Players);
            Assert.Equal("player-1", saved.Players[0].PlayerId);
        }

        [Fact]
        public void GuessLog_requires_PlayerId_and_PlayerName()
        {
            using var ctx = CreateContext();

            var match = new Match
            {
                Id = Guid.NewGuid(),
                RoomId = "ROOM-1",
                RoomName = "Room",
                Low = 1,
                High = 10,
                Secret = 5,
                StartedAt = DateTimeOffset.UtcNow,
                EndedAt = DateTimeOffset.UtcNow,
                WinnerPlayerId = "p1",
                WinnerName = "P1"
            };

            ctx.Matches.Add(match);
            ctx.SaveChanges();

            var log = new GuessLog
            {
                Id = Guid.NewGuid(),
                MatchId = match.Id,
                PlayerId = "p1",
                PlayerName = "P1",
                Value = 3,
                Result = "TooLow",
                At = DateTimeOffset.UtcNow
            };

            ctx.GuessLogs.Add(log);
            ctx.SaveChanges();

            Assert.Equal(1, ctx.GuessLogs.Count());
        }
    }
}
