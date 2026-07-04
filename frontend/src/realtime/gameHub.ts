import * as signalR from '@microsoft/signalr'

export interface ChatMessage {
  playerName: string
  text: string
  sentAt: string
  system?: boolean
  roll?: DiceRollEvent
}

export function createGameConnection() {
  return new signalR.HubConnectionBuilder()
    .withUrl('/hub/game')
    .withAutomaticReconnect()
    .build()
}
export interface DiceRollEvent {
  notation: string
  rolls: number[]
  modifier: number
  total: number
}