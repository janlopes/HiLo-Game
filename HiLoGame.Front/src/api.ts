
import type { GameRoom } from './types'

const base = import.meta.env.VITE_API_BASE_URL ?? '' // empty because vite proxy maps /api to backend

async function http<T>(input: RequestInfo, init?: RequestInit): Promise<T> {
  const res = await fetch(input, {
    headers: { 'Content-Type': 'application/json', ...(init?.headers || {}) },
    ...init,
  })
  if (!res.ok) {
    const text = await res.text()
    throw new Error(text || `${res.status} ${res.statusText}`)
  }
  if (res.status === 204) return undefined as T
  return res.json() as Promise<T>
}

export const api = {
  createRoom: (name: string, low: number, high: number, secret?: number) =>
    http<GameRoom>(`${base}/api/rooms/create`, {
      method: 'POST',
      body: JSON.stringify({ name, low, high, secret }),
    }),

  getRoom: (roomIdOrName: string) =>
    http<GameRoom>(`${base}/api/rooms/${encodeURIComponent(roomIdOrName)}`),

  joinRoom: (roomId: string, playerId: string, name: string) =>
    http<GameRoom>(`${base}/api/rooms/${encodeURIComponent(roomId)}/join`, {
      method: 'POST',
      body: JSON.stringify({ playerId, playerName: name }),
    }),

  startMatch: (roomId: string) =>
    http<GameRoom>(`${base}/api/game/${encodeURIComponent(roomId)}/start`, { method: 'POST' }),

  makeGuess: (roomId: string, playerId: string, value: number) =>
    http<{ result: 'TooLow' | 'TooHigh' | 'Correct'; room: GameRoom }>(
      `${base}/api/game/${encodeURIComponent(roomId)}/guess`,
      { method: 'POST', body: JSON.stringify({ playerId, value }) }
    ),

  voteRematch: (roomId: string, playerId: string, yes: boolean) =>
    http<GameRoom>(`${base}/api/rooms/${encodeURIComponent(roomId)}/vote-rematch`, {
      method: 'POST',
      body: JSON.stringify({ playerId, yes })
    }),
}