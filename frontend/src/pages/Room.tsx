import { useEffect, useRef, useState } from 'react'
import { Link, useParams } from 'react-router-dom'
import * as signalR from '@microsoft/signalr'
// Добавили импорт типа DiceRollEvent в первую строку:
import { createGameConnection, type ChatMessage, type DiceRollEvent } from '../realtime/gameHub'

export default function Room() {
  const { joinCode } = useParams()
  const [playerName, setPlayerName] = useState('')
  const [joined, setJoined] = useState(false)
  const [messages, setMessages] = useState<ChatMessage[]>([])
  const [text, setText] = useState('')
  const connectionRef = useRef<signalR.HubConnection | null>(null)
  const bottomRef = useRef<HTMLDivElement>(null)

  useEffect(() => {
    bottomRef.current?.scrollIntoView({ behavior: 'smooth' })
  }, [messages])

  async function handleJoin() {
    if (!playerName.trim() || !joinCode) return

    const connection = createGameConnection()

    connection.on('MessageReceived', (playerName: string, text: string, sentAt: string) => {
      setMessages((prev) => [...prev, { playerName, text, sentAt }])
    })

    connection.on('PlayerJoined', (name: string) => {
      setMessages((prev) => [...prev, system(`${name} присоединился`)])
    })

    connection.on('PlayerLeft', (name: string) => {
      setMessages((prev) => [...prev, system(`${name} вышел`)])
    })
    connection.on('DiceRolled', (playerName: string, roll: DiceRollEvent) => {
      setMessages((prev) => [...prev, {
        playerName,
        text: '',
        sentAt: new Date().toISOString(),
        roll,
      }])
    })
    connection.onreconnected(async () => {
    await connection.invoke('JoinRoom', joinCode, playerName.trim())
      setMessages((prev) => [...prev, system('соединение восстановлено')])
    })
    await connection.start()
    await connection.invoke('JoinRoom', joinCode, playerName.trim())

    connectionRef.current = connection
    setJoined(true)
  }

  useEffect(() => {
    return () => {
      connectionRef.current?.stop()
    }
  }, [])

  async function handleSend() {
    if (!text.trim() || !connectionRef.current) return
    await connectionRef.current.invoke('SendMessage', text.trim())
    setText('')
  }

  function system(text: string): ChatMessage {
    return { playerName: '', text, sentAt: new Date().toISOString(), system: true }
  }

  if (!joined) {
    return (
      <div className="room">
        <header>
          <Link to="/">← Лобби</Link>
          <h1>Комната <code>{joinCode}</code></h1>
        </header>
        <section className="card">
          <h2>Представься</h2>
          <div className="row">
            <input
              value={playerName}
              onChange={(e) => setPlayerName(e.target.value)}
              placeholder="Имя персонажа"
              onKeyDown={(e) => e.key === 'Enter' && handleJoin()}
            />
            <button onClick={handleJoin}>За стол</button>
          </div>
        </section>
      </div>
    )
  }

  return (
    <div className="room">
      <header>
        <Link to="/">← Лобби</Link>
        <h1>Комната <code>{joinCode}</code></h1>
      </header>

      <section className="card chat">
        <div className="chat-messages">
          {/* ОБНОВЛЕННЫЙ БЛОК ОТОБРАЖЕНИЯ СООБЩЕНИЙ: */}
          {messages.map((m, i) =>
            m.roll ? (
              <p key={i} className="chat-roll">
                <strong>{m.playerName}</strong> бросает {m.roll.notation}: [{m.roll.rolls.join(', ')}]
                {m.roll.modifier !== 0 && (m.roll.modifier > 0 ? ` + ${m.roll.modifier}` : ` − ${-m.roll.modifier}`)}
                {' '} = <strong>{m.roll.total}</strong>
              </p>
            ) : m.system ? (
              <p key={i} className="chat-system">{m.text}</p>
            ) : (
              <p key={i}>
                <strong>{m.playerName}:</strong> {m.text}
              </p>
            )
          )}
          <div ref={bottomRef} />
        </div>
        <div className="row">
          <input
            value={text}
            onChange={(e) => setText(e.target.value)}
            placeholder="Сообщение..."
            onKeyDown={(e) => e.key === 'Enter' && handleSend()}
          />
          <button onClick={handleSend}>➤</button>
        </div>
      </section>
    </div>
  )
}