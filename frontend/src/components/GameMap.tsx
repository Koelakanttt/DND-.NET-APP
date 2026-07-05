import { useRef, useState } from 'react'
import * as signalR from '@microsoft/signalr'

export interface TokenDto {
  id: string
  name: string
  color: string
  x: number
  y: number
}

interface Props {
  joinCode: string
  mapUrl: string | null
  tokens: TokenDto[]
  connection: signalR.HubConnection
  onLocalMove: (id: string, x: number, y: number) => void
}

export default function GameMap({ joinCode, mapUrl, tokens, connection, onLocalMove }: Props) {
  const mapRef = useRef<HTMLDivElement>(null)
  const dragId = useRef<string | null>(null)
  const lastSent = useRef(0)
  const [uploading, setUploading] = useState(false)

  function toPercent(e: React.PointerEvent) {
    const rect = mapRef.current!.getBoundingClientRect()
    return {
      x: Math.min(100, Math.max(0, ((e.clientX - rect.left) / rect.width) * 100)),
      y: Math.min(100, Math.max(0, ((e.clientY - rect.top) / rect.height) * 100)),
    }
  }

  function onPointerDown(e: React.PointerEvent, id: string) {
    dragId.current = id
    ;(e.target as HTMLElement).setPointerCapture(e.pointerId)
  }

  function onPointerMove(e: React.PointerEvent) {
    if (!dragId.current) return
    const { x, y } = toPercent(e)
    onLocalMove(dragId.current, x, y)          // мгновенно у себя
    const now = Date.now()
    if (now - lastSent.current > 66) {          // ~15 сообщений/сек, не чаще
      lastSent.current = now
      connection.invoke('MoveToken', dragId.current, x, y)
    }
  }

  function onPointerUp(e: React.PointerEvent) {
    if (!dragId.current) return
    const { x, y } = toPercent(e)
    onLocalMove(dragId.current, x, y)
    connection.invoke('DropToken', dragId.current, x, y)  // фиксация в базе
    dragId.current = null
  }

  async function uploadMap(file: File) {
    setUploading(true)
    try {
      const form = new FormData()
      form.append('file', file)
      const res = await fetch(`/api/rooms/${joinCode}/map`, { method: 'POST', body: form })
      if (!res.ok) alert(await res.text())
      // новый mapUrl прилетит всем через SignalR? нет — простоты ради перезагрузим состояние сами:
      // проще всего: сервер мог бы разослать MapState, но пока — локально:
      const data = await res.json()
      window.dispatchEvent(new CustomEvent('map-updated', { detail: data.mapUrl }))
    } finally {
      setUploading(false)
    }
  }

  return (
    <div className="map-panel">
      <div className="map-toolbar">
        <label className="upload-btn">
          {uploading ? 'Загрузка...' : '🗺 Загрузить карту'}
          <input type="file" accept="image/png,image/jpeg,image/webp" hidden
            onChange={(e) => e.target.files?.[0] && uploadMap(e.target.files[0])} />
        </label>
        <button onClick={() => {
          const name = prompt('Имя токена:')
          if (name) connection.invoke('AddToken', name,
            '#' + Math.floor(Math.random() * 0xffffff).toString(16).padStart(6, '0'))
        }}>➕ Токен</button>
      </div>

      <div ref={mapRef} className="map-area"
        style={mapUrl ? { backgroundImage: `url(${mapUrl})` } : undefined}
        onPointerMove={onPointerMove} onPointerUp={onPointerUp}>
        {!mapUrl && <p className="map-empty">Карта не загружена</p>}
        {tokens.map((t) => (
          <div key={t.id} className="token"
            style={{ left: `${t.x}%`, top: `${t.y}%`, background: t.color }}
            onPointerDown={(e) => onPointerDown(e, t.id)}
            onDoubleClick={() => confirm(`Удалить ${t.name}?`) && connection.invoke('RemoveToken', t.id)}
            title={t.name}>
            {t.name.slice(0, 2).toUpperCase()}
          </div>
        ))}
      </div>
    </div>
  )
}