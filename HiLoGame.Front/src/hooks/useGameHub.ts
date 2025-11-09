import { useEffect, useRef } from 'react'
import * as signalR from '@microsoft/signalr'

export function useGameHub(onRoomUpdated: (room: any) => void) {
  const connectionRef = useRef<signalR.HubConnection | null>(null)

  useEffect(() => {
    const conn = new signalR.HubConnectionBuilder()
      .withUrl('/hubs/game')
      .withAutomaticReconnect()
      .build()

    connectionRef.current = conn

    conn.on('RoomUpdated', (room) => onRoomUpdated(room))

    conn.start().catch(console.error)

    //conn.invoke('JoinRoomGroup', 'r').catch(console.error)

    return () => {
      conn.stop().catch(() => {})
    }
  }, [onRoomUpdated])
}