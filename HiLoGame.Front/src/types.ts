export type RoomStatus = 'Lobby' | 'InProgress' | 'Finished'

export interface Player {
  playerId: string
  name: string
  wantsRematch?: boolean
}

export interface Guess {
  playerId: string
  playerName: string
  value: number
  result: 'TooLow' | 'TooHigh' | 'Correct'
  at: string
}

export interface GameRoom {
  roomId: string
  name: string
  low: number
  high: number
  secret?: number
  status: number
  players: Player[]
  currentPlayer?: Player
  guesses: Guess[]
  winnerPlayerId?: string
  winnerName?: string
}

export interface ApiError { message: string }