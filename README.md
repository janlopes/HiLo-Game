# Hi-Lo Game — Multiplayer (.NET 8 + React)

A clean, testable implementation of the classic **Hi–Lo** guessing game with a **multiplayer** spin. The solution is split into backend (ASP.NET Core + EF Core + SignalR) and a lightweight React frontend (Vite).

## Solution structure

```
Hi-Lo-Game.sln
├─ Hi-Lo-Game/                 # ASP.NET Core Web API (Swagger + SignalR hub)
│  ├─ Controllers/             # Game, Rooms, and Matches endpoints
│  ├─ Hubs/GameHub.cs          # SignalR hub for real-time room updates
│  ├─ Program.cs               # Composition root and app startup
│  └─ appsettings.json         # SQLite connection string (Data Source=hilo.db)
├─ HiLoGame.Application/       # Core game logic and services
│  ├─ Models/                  # GameRoom, Player, Guess, enums
│  └─ Services/                # GameService, RoomService, MatchService
├─ HiLoGame.Infrastructure/    # EF Core + repository + cache-backed state store
│  ├─ Data/GameDbContext.cs    # DbContext + entity mappings
│  ├─ Persistence/             # MatchRepository for finished matches
│  └─ State/                   # IDistributedCache-based IStateStore
├─ HiLoGame.Front/             # React 18 + Vite + SignalR client
│  └─ src/                     # Minimal UI for creating/joining rooms and playing
└─ HiLoGame.Tests.Unit/        # Unit tests for core services
```

## Prerequisites

- **.NET 8 SDK**
- **Node 18+** (or 20+) with **npm** or **pnpm**

## Backend — run

```bash
cd Hi-Lo-Game/Hi-Lo-Game
dotnet restore
dotnet run
```

- SQLite database file `hilo.db` will be created automatically on first run (via `EnsureCreated`).
- Swagger UI at `http://localhost:5240/swagger` (actual port printed on run).

### Important endpoints

- `POST /api/rooms/create` → Create a room (with optional min/max and secret for testing).
- `POST /api/game/{roomId}/start` → Start a match in a room.
- `POST /api/game/{roomId}/join` → Join room with `playerId` and `playerName`.
- `POST /api/game/{roomId}/guess` → Submit a guess `{ playerId, value }`.
- `POST /api/game/{roomId}/vote-rematch` → Players consent to rematch after finish.
- `GET  /api/matches` and `GET /api/matches/{id}` → Read finished matches + guess logs.

**Real‑time:** SignalR hub at `/hubs/game`. Clients join a group `room:{roomId}` and receive `turnChanged` and other updates.

## Frontend — run (dev)

```bash
cd Hi-Lo-Game/HiLoGame.Front
npm install
npm run dev
```
Vite dev server proxies `/api` → backend, so no CORS setup is needed in dev. Configure `VITE_API_BASE_URL` if you deploy the API separately.

## Gameplay rules implemented

- The server chooses a **secret number in the inclusive range [Min, Max]** when a room is created or a match starts.
- Each submitted guess returns one of `TooLow`, `TooHigh`, or `Correct` and **the turn rotates to the next player**.
- The **match ends** as soon as a guess is `Correct`. The winner and the full guess log are **persisted** (SQLite).
- **Multiplayer**: multiple players can **join the same room**, with **turn‑based** order enforced and **SignalR** broadcasts to the room group.
- **Rematch**: after finish, players can vote; if everyone votes yes, the room resets for a new match (optionally reseeding the secret).

## Tests

- `HiLoGame.Tests.Unit` covers `GameService` and `RoomService` behavior (room creation, joining, guessing outcomes, rotation, and finish).  
Run:
```bash
dotnet test
```

## Notes & assumptions

- Persistence of finished matches (summary + per‑guess logs) uses EF Core **SQLite** with auto‑create on startup.
- Room state is kept in an **IDistributedCache**‑backed store for simplicity. Swap for Redis for production.
- The frontend is intentionally minimal to highlight the API and real‑time flow; it can be replaced by any client.

## Quick API usage sample

1) Create a room:
```http
POST /api/rooms/create
{ "name": "my-room", "low": 1, "high": 100 }
```

2) Join as Alice:
```http
POST /api/game/my-room/join
{ "playerId": "p1", "playerName": "Alice" }
```

3) Start:
```http
POST /api/game/my-room/start
```

4) Guess:
```http
POST /api/game/my-room/guess
{ "playerId": "p1", "value": 42 }
```

5) Vote rematch:
```http
POST /api/game/my-room/vote-rematch
{ "playerId": "p1", "want": true }
```

---
