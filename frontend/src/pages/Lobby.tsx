import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { createRoom, findRoom, getRooms, type RoomResponse } from '../api/rooms'

export default function Lobby() {
  const navigate = useNavigate()
  const [rooms, setRooms] = useState<RoomResponse[]>([])
  const [name, setName] = useState('')
  const [code, setCode] = useState('')
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(false)

  useEffect(() => {
    getRooms().then(setRooms).catch(() => setError('Не удалось загрузить список комнат'))
  }, [])

  async function handleCreate() {
    if (!name.trim()) return
    setLoading(true)
    setError('')
    try {
      const room = await createRoom(name.trim())
      navigate(`/room/${room.joinCode}`)
    } catch {
      setError('Не удалось создать комнату')
    } finally {
      setLoading(false)
    }
  }

  async function handleJoin() {
    if (!code.trim()) return
    setLoading(true)
    setError('')
    try {
      const room = await findRoom(code.trim().toUpperCase())
      if (!room) {
        setError('Комната не найдена')
        return
      }
      navigate(`/room/${room.joinCode}`)
    } catch {
      setError('Ошибка при поиске комнаты')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="lobby">
      <h1>🎲 TableTop</h1>

      <section className="card">
        <h2>Создать комнату</h2>
        <div className="row">
          <input
            value={name}
            onChange={(e) => setName(e.target.value)}
            placeholder="Название сессии"
            onKeyDown={(e) => e.key === 'Enter' && handleCreate()}
          />
          <button onClick={handleCreate} disabled={loading}>Создать</button>
        </div>
      </section>

      <section className="card">
        <h2>Войти по коду</h2>
        <div className="row">
          <input
            value={code}
            onChange={(e) => setCode(e.target.value.toUpperCase())}
            placeholder="X7KQ2M"
            maxLength={6}
            onKeyDown={(e) => e.key === 'Enter' && handleJoin()}
          />
          <button onClick={handleJoin} disabled={loading}>Войти</button>
        </div>
      </section>

      {error && <p className="error">{error}</p>}

      {rooms.length > 0 && (
        <section className="card">
          <h2>Комнаты</h2>
          <ul className="room-list">
            {rooms.map((r) => (
              <li key={r.id} onClick={() => navigate(`/room/${r.joinCode}`)}>
                <span>{r.name}</span>
                <code>{r.joinCode}</code>
              </li>
            ))}
          </ul>
        </section>
      )}
    </div>
  )
}