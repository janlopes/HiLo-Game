using HiLoGame.Application.Abstractions;
using HiLoGame.Application.Models;
using HiLoGame.Application.Services;

namespace HiLoGame.Tests.Application
{
    public class GameServiceTests
    {
        private sealed class FakeRoomService : IRoomService
        {
            private readonly System.Collections.Concurrent.ConcurrentDictionary<string, GameRoom> _store = new();
            public string BuildRoomKey(string roomId) => $"room:{roomId}";
            public Task SaveAsync(GameRoom room, CancellationToken ct = default)
            {
                _store[room.RoomId] = room;
                _store[room.Name] = room; // allow lookup both by RoomId and Name (controllers use both)
                return Task.CompletedTask;
            }
            public Task<GameRoom?> TryGetAsync(string roomId, CancellationToken ct = default)
            {
                _store.TryGetValue(roomId, out var room);
                return Task.FromResult(room);
            }
            public Task DeleteAsync(string roomId, CancellationToken ct = default)
            {
                _store.TryRemove(roomId, out _);
                return Task.CompletedTask;
            }
        }

        private sealed class FakeMatchService : IMatchService
        {
            public int PersistCalls { get; private set; }
            public Task PersistFinishedMatchAsync(GameRoom room, CancellationToken ct = default)
            {
                PersistCalls++;
                return Task.CompletedTask;
            }
        }

        private static GameRoom NewRoom(string id = "r1", string name = "Room", int low = 1, int high = 100, int secret = 50)
        {
            return new GameRoom
            {
                RoomId = id,
                Name = name,
                Low = low,
                High = high,
                Mystery = secret,
                Status = RoomStatus.Lobby
            };
        }

        [Fact]
        public async Task CreateRoomAsync_throws_if_low_ge_high()
        {
            var rooms = new FakeRoomService();
            var matches = new FakeMatchService();
            var svc = new GameService(rooms, matches);

            await Assert.ThrowsAsync<ArgumentException>(() =>
                svc.CreateRoomAsync("R", 10, 10, null));
        }

        [Fact]
        public async Task CreateRoomAsync_saves_room_and_returns_it()
        {
            var rooms = new FakeRoomService();
            var matches = new FakeMatchService();
            var svc = new GameService(rooms, matches);

            var room = await svc.CreateRoomAsync("Lobby", 1, 10, 5);

            Assert.Equal("Lobby", room.Name);
            Assert.Equal(1, room.Low);
            Assert.Equal(10, room.High);
            Assert.Equal(RoomStatus.Lobby, room.Status);

            var loaded = await rooms.TryGetAsync(room.RoomId);
            Assert.NotNull(loaded);
            Assert.Equal(room.RoomId, loaded!.RoomId);
        }

        [Fact]
        public async Task JoinRoomAsync_adds_player_in_lobby()
        {
            var rooms = new FakeRoomService();
            var svc = new GameService(rooms, new FakeMatchService());

            var room = NewRoom();
            await rooms.SaveAsync(room);

            var updated = await svc.JoinRoomAsync(room.RoomId, "p1", "Alice");

            Assert.Single(updated.Players);
            Assert.Equal("p1", updated.Players[0].PlayerId);
        }

        [Fact]
        public async Task JoinRoomAsync_throws_if_match_already_started()
        {
            var rooms = new FakeRoomService();
            var svc = new GameService(rooms, new FakeMatchService());

            var room = NewRoom();
            room.Status = RoomStatus.InProgress;
            await rooms.SaveAsync(room);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                svc.JoinRoomAsync(room.RoomId, "p1", "Alice"));
        }

        [Fact]
        public async Task StartMatchAsync_sets_status_inprogress_and_keeps_current_player()
        {
            var rooms = new FakeRoomService();
            var svc = new GameService(rooms, new FakeMatchService());

            var room = NewRoom(low: 1, high: 10, secret: 7);
            room.Players.Add(new Player { PlayerId = "p1", Name = "Alice" });
            room.Players.Add(new Player { PlayerId = "p2", Name = "Bob" });
            await rooms.SaveAsync(room);

            var after = await svc.StartMatchAsync(room.RoomId);

            Assert.Equal(RoomStatus.InProgress, after.Status);
            Assert.NotNull(after.CurrentPlayer);
            Assert.True(after.Mystery >= after.Low && after.Mystery <= after.High);
        }

        [Fact]
        public async Task MakeGuess_returns_TooLow_or_TooHigh_until_correct()
        {
            var rooms = new FakeRoomService();
            var matches = new FakeMatchService();
            var svc = new GameService(rooms, matches);

            var room = NewRoom(secret: 42);
            room.Players.Add(new Player { PlayerId = "p1", Name = "Alice" });
            room.Players.Add(new Player { PlayerId = "p2", Name = "Bob" });
            room.Status = RoomStatus.InProgress;
            await rooms.SaveAsync(room);

            var (r1, rm1) = await svc.MakeGuessAsync(room.RoomId, "p1", 10);
            Assert.Equal(GuessResult.TooLow, r1);
            Assert.Equal(1, rm1.Guesses.Count);

            var (r2, rm2) = await svc.MakeGuessAsync(room.RoomId, "p2", 90);
            Assert.Equal(GuessResult.TooHigh, r2);
            Assert.Equal(2, rm2.Guesses.Count);
        }

        [Fact]
        public async Task MakeGuess_correct_finishes_and_persists_match()
        {
            var rooms = new FakeRoomService();
            var matches = new FakeMatchService();
            var svc = new GameService(rooms, matches);

            var room = NewRoom(secret: 5);
            room.Players.Add(new Player { PlayerId = "p1", Name = "Alice" });
            room.Players.Add(new Player { PlayerId = "p2", Name = "Bob" });
            room.Status = RoomStatus.InProgress;
            await rooms.SaveAsync(room);

            var (res, after) = await svc.MakeGuessAsync(room.RoomId, "p1", 5);

            Assert.Equal(GuessResult.Correct, res);
            Assert.Equal(RoomStatus.Finished, after.Status);
            Assert.Equal(1, matches.PersistCalls);
        }

        [Fact]
        public async Task VoteRematch_two_yes_resets_room_and_clears_flags()
        {
            var rooms = new FakeRoomService();
            var svc = new GameService(rooms, new FakeMatchService());

            var room = NewRoom(secret: 7);
            room.Players.Add(new Player { PlayerId = "p1", Name = "Alice" });
            room.Players.Add(new Player { PlayerId = "p2", Name = "Bob" });
            room.Status = RoomStatus.Finished;
            room.Guesses.Add(new Guess { PlayerId = "p1", Value = 7, At = DateTimeOffset.UtcNow, Result = GuessResult.Correct });
            await rooms.SaveAsync(room);

            // first vote
            var after1 = await svc.VoteRematchAsync(room.RoomId, "p1", true);
            Assert.True(after1.Players.First(p => p.PlayerId == "p1").WantsRematch);
            Assert.Equal(RoomStatus.Finished, after1.Status);

            // second vote triggers reset
            var after2 = await svc.VoteRematchAsync(room.RoomId, "p2", true);
            Assert.Equal(RoomStatus.InProgress, after2.Status);
            Assert.Empty(after2.Guesses);
            Assert.True(after2.Players.All(p => !p.WantsRematch));
        }
    }
}
