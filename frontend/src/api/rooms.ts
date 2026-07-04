export interface RoomResponse {
  id: string
  name: string
  joinCode: string
  createdAt: string
}

export async function createRoom(name: string): Promise<RoomResponse> {
  const res = await fetch('/api/rooms', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ name }),
  })
  if (!res.ok) throw new Error(await res.text())
  return res.json()
}

export async function getRooms(): Promise<RoomResponse[]> {
  const res = await fetch('/api/rooms')
  if (!res.ok) throw new Error(await res.text())
  return res.json()
}

export async function findRoom(joinCode: string): Promise<RoomResponse | null> {
  const res = await fetch(`/api/rooms/${encodeURIComponent(joinCode)}`)
  if (res.status === 404) return null
  if (!res.ok) throw new Error(await res.text())
  return res.json()
}