
import { useCallback, useEffect, useMemo, useState } from 'react'
import { api } from './api'
import type { GameRoom, Player } from './types'
import { useGameHub } from './hooks/useGameHub'

export default function App() {
  const [room, setRoom] = useState<GameRoom | null>(null)
  const [playerId, setPlayerId] = useState<string>('1')
  const [playerName, setPlayerName] = useState<string>('Edson')
  const [busy, setBusy] = useState(false)
  const [error, setError] = useState<string | null>(null)

  useGameHub((updated) => {
    if (room && updated.roomId === room.roomId) {
      setRoom(updated)
    }
  })

  const isInRoom = !!room
  const myPlayer: Player | undefined = useMemo(
    () => room?.players?.find(p => p.playerId === playerId),
    [room, playerId]
  )

  const handle = useMemo(() => ({
    async create(name: string, low: number, high: number, secret?: number) {
      setBusy(true); setError(null)
      try { const r = await api.createRoom(name, low, high, secret); setRoom(r) } catch (e: any) { setError(e.message) } finally { setBusy(false) }
    },
    async get(idOrName: string) {
      setBusy(true); setError(null)
      console.log('Getting room', idOrName);
      try { const r = await api.getRoom(idOrName); setRoom(r) } catch (e: any) { setError(e.message) } finally { setBusy(false) }
    },
    async join(roomId: string) {
      setBusy(true); setError(null)
      try { const r = await api.joinRoom(roomId, playerId, playerName); setRoom(r) } catch (e: any) { setError(e.message) } finally { setBusy(false) }
    },
    async start(roomId: string) {
      setBusy(true); setError(null)
      try { const r = await api.startMatch(roomId); setRoom(r) } catch (e: any) { setError(e.message) } finally { setBusy(false) }
    },
    async guess(roomId: string, value: number) {
      setBusy(true); setError(null)
      try {
        const { room: r } = await api.makeGuess(roomId, playerId, value);
        console.log('Guess result:', r);
        setRoom(r)
      } catch (e: any) { setError(e.message) } finally { setBusy(false) }
    },
    async vote(roomId: string, yes: boolean) {
      setBusy(true); setError(null)
      try { const r = await api.voteRematch(roomId, playerId, yes); setRoom(r) } catch (e: any) { setError(e.message) } finally { setBusy(false) }
    }
  }), [playerId, playerName, room])

  return (
    <div style={{ maxWidth: 900, margin: '0 auto', padding: 16, fontFamily: 'system-ui, sans-serif' }}>
      <h1>HiLo Game</h1>

      <section style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 16 }}>
        <div>
          <h3>Create room</h3>
          <CreateRoomForm onCreate={(n, l, h, s) => handle.create(n, l, h, s)} busy={busy} />
        </div>
        <div>
          <h3>Find / Join</h3>
          <JoinRoomForm onFind={handle.get} onJoin={handle.join} current={room?.roomId} />
        </div>
      </section>

      <hr style={{ margin: '16px 0' }} />

      <section>
        <h3>Your identity</h3>
        <div style={{ display: 'flex', gap: 8, alignItems: 'center' }}>
          <label>PlayerId</label>
          <input value={playerId} onChange={e => setPlayerId(e.target.value)} />
          <label>Name</label>
          <input value={playerName} onChange={e => setPlayerName(e.target.value)} />
        </div>
      </section>

      {error && <div style={{ background: '#fee', border: '1px solid #f99', padding: 8, marginTop: 8 }}>{error}</div>}

      <hr style={{ margin: '16px 0' }} />

      <section>
        <h2>Room</h2>
        {!isInRoom && <p>No room loaded yet.</p>}
        {room && <RoomPanel room={room} busy={busy} onStart={() => handle.start(room.roomId)} onGuess={(v) => handle.guess(room.roomId, v)} onVote={(yes) => handle.vote(room.roomId, yes)} onGet={(v) => handle.get(room.roomId)} me={myPlayer} />}
      </section>
    </div>
  )
}

function CreateRoomForm({ onCreate, busy }: { onCreate: (name: string, low: number, high: number, secret?: number) => void, busy: boolean }) {
  const [name, setName] = useState('Lobby')
  const [low, setLow] = useState(1)
  const [high, setHigh] = useState(100)
  const [secret, setSecret] = useState<string>('')
  return (
    <form onSubmit={e => { e.preventDefault(); onCreate(name, low, high, secret ? Number(secret) : undefined) }}>
      <div style={{ display: 'grid', gap: 8 }}>
        <input placeholder="Room name" value={name} onChange={e => setName(e.target.value)} />
        <div style={{ display: 'flex', gap: 8 }}>
          <input type="number" value={low} onChange={e => setLow(Number(e.target.value))} placeholder="Low" />
          <input type="number" value={high} onChange={e => setHigh(Number(e.target.value))} placeholder="High" />
          <input type="number" value={secret} onChange={e => setSecret(e.target.value)} placeholder="Secret (optional)" />
        </div>
        <button disabled={busy} type="submit">Create</button>
      </div>
    </form>
  )
}

function JoinRoomForm({ onFind, onJoin, current }: { onFind: (idOrName: string) => void, onJoin: (roomId: string) => void, current?: string }) {
  const [q, setQ] = useState('')
  return (
    <div style={{ display: 'grid', gap: 8 }}>
      <form onSubmit={(e) => { e.preventDefault(); onFind(q) }}>
        <input placeholder="Room id or name" value={q} onChange={e => setQ(e.target.value)} />
        <div style={{ display: 'flex', gap: 8, marginTop: 8 }}>
          <button type="submit">Load</button>
          {current && <button type="button" onClick={() => onJoin(current)}>Join current</button>}
        </div>
      </form>
    </div>
  )
}

function RoomPanel({ room, me, busy, onStart, onGuess, onVote, onGet }: { room: GameRoom, me?: Player, busy: boolean, onStart: () => void, onGuess: (v: number) => void, onVote: (yes: boolean) => void, onGet: (v: string) => void, }) {
  const [guess, setGuess] = useState('')
  const inLobby = room.status === 0
  const inProgress = room.status === 1
  const finished = room.status === 2

  const amCurrent = !!me && room.currentPlayer && room.currentPlayer.playerId === me.playerId

  return (
    <div style={{ border: '1px solid #ddd', borderRadius: 8, padding: 12 }}>
      <p><b>{room.name}</b> — id: <code>{room.roomId}</code></p>
      <p>Status: <b>{room.status}</b> | Range: {room.low} to {room.high}</p>

      <div style={{ display: 'flex', gap: 16 }}>
        <div>
          <h4>Players</h4>
          <ul>
            {room?.players?.map(p => (
              <li key={p.playerId}>{p.name} {room.currentPlayer?.playerId === p.playerId ? '(current)' : ''} {p.wantsRematch ? '👍' : ''}</li>
            ))}
          </ul>
        </div>
        <div>
          <h4>Guesses</h4>
          <ol>
            {room?.guesses?.map((g, i) => (
              <li key={i}>{g.playerId} - {g.playerName} guessed {g.value} → {g.result} @ {new Date(g.at).toLocaleTimeString()}</li>
            ))}
          </ol>
        </div>
      </div>

      <div style={{ display: 'flex', gap: 8, marginTop: 8 }}>
        {inLobby && <button disabled={busy || room.players.length === 0} onClick={onStart}>Start match</button>}
        {inProgress && (
          <form onSubmit={e => { e.preventDefault(); const v = Number(guess); if (!Number.isNaN(v)) onGuess(v); console.log('aaaaa'), onGet(room.roomId); setGuess('') }}>
            <input type="number" value={guess} onChange={e => setGuess(e.target.value)} placeholder="Enter your guess" />
            <button disabled={busy || !amCurrent} type="submit">Guess</button>
          </form>
        )}
        {finished && (
          <div style={{ display: 'flex', gap: 8 }}>
            <button disabled={busy} onClick={() => onVote(true)}>Vote rematch 👍</button>
            <button disabled={busy} onClick={() => onVote(false)}>Vote no 👎</button>
          </div>
        )}
      </div>
    </div>
  )
}
