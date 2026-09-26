export interface Session {
  name: string
  email: string
}

const sessionKey = 'vatio.session'

export function readSession(): Session | null {
  try {
    const stored = window.localStorage.getItem(sessionKey)
    return stored ? (JSON.parse(stored) as Session) : null
  } catch {
    return null
  }
}

export function saveSession(session: Session | null) {
  try {
    if (session) window.localStorage.setItem(sessionKey, JSON.stringify(session))
    else window.localStorage.removeItem(sessionKey)
  } catch {
    return
  }
}
