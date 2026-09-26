import { createContext, useContext, useState } from 'react'
import type { ReactNode } from 'react'
import { demoAccount } from './demoAccount'
import { readSession, saveSession } from './sessionStorage'
import type { Session } from './sessionStorage'

interface AuthContextValue {
  session: Session | null
  signIn: (email: string, password: string) => Promise<void>
  signOut: () => void
}

const AuthContext = createContext<AuthContextValue | null>(null)

export function AuthProvider({ children }: { children: ReactNode }) {
  const [session, setSession] = useState<Session | null>(readSession)

  async function signIn(email: string, password: string) {
    const isDemoAccount = email.trim().toLowerCase() === demoAccount.email && password === demoAccount.password
    if (!isDemoAccount) {
      throw new Error('Correo o contraseña incorrectos. Revisa los datos e inténtalo de nuevo.')
    }

    const newSession = { name: demoAccount.name, email: demoAccount.email }
    saveSession(newSession)
    setSession(newSession)
  }

  function signOut() {
    saveSession(null)
    setSession(null)
  }

  return <AuthContext.Provider value={{ session, signIn, signOut }}>{children}</AuthContext.Provider>
}

export function useAuth() {
  const context = useContext(AuthContext)
  if (!context) throw new Error('useAuth debe usarse dentro de AuthProvider')
  return context
}
