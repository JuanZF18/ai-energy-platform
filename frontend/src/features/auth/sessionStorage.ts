export interface Session {
  name: string
  email: string
}

export interface StoredDemoSession extends Session {
  token: string
}

const sessionKey = 'vatio.session'

export function readDemoSession(): StoredDemoSession | null {
  try {
    const stored = window.localStorage.getItem(sessionKey)
    const session = stored ? (JSON.parse(stored) as Partial<StoredDemoSession>) : null
    return session?.token && session.email && session.name ? (session as StoredDemoSession) : null
  } catch {
    return null
  }
}

export function saveDemoSession(session: StoredDemoSession | null) {
  try {
    if (session) window.localStorage.setItem(sessionKey, JSON.stringify(session))
    else window.localStorage.removeItem(sessionKey)
  } catch {
    return
  }
}
