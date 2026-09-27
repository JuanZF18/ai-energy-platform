import { createContext, useContext, useEffect, useRef, useState } from 'react'
import type { ReactNode } from 'react'
import { useQuery, useQueryClient } from '@tanstack/react-query'
import { ApiError, api, configureApiAuth } from '@/lib/api'
import type { ClientConfig } from '@/lib/types'
import { connectFirebase } from './firebaseSession'
import type { FirebaseSession } from './firebaseSession'
import { readDemoSession, saveDemoSession } from './sessionStorage'
import type { Session, StoredDemoSession } from './sessionStorage'

type AuthStatus = 'loading' | 'ready' | 'unavailable'

interface AuthContextValue {
  status: AuthStatus
  session: Session | null
  demoAccount: ClientConfig['demoAccount'] | null
  signIn: (email: string, password: string) => Promise<void>
  signOut: () => void
  retry: () => void
}

const AuthContext = createContext<AuthContextValue | null>(null)

export function AuthProvider({ children }: { children: ReactNode }) {
  const queryClient = useQueryClient()
  const config = useQuery({ queryKey: ['config'], queryFn: api.config, staleTime: Infinity, retry: 2 })
  const firebase = useRef<FirebaseSession | null>(null)
  const demoSession = useRef<StoredDemoSession | null>(readDemoSession())
  const [session, setSession] = useState<Session | null>(null)
  const [isRestoring, setIsRestoring] = useState(true)

  const mode = config.data?.authMode

  useEffect(() => {
    if (!config.data) return

    if (config.data.authMode === 'DEMO' || !config.data.firebase) {
      const stored = demoSession.current
      setSession(stored ? { name: stored.name, email: stored.email } : null)
      setIsRestoring(false)
      return
    }

    firebase.current ??= connectFirebase(config.data.firebase)
    return firebase.current.watch((user) => {
      setSession(user?.email ? { name: user.displayName ?? user.email, email: user.email } : null)
      setIsRestoring(false)
    })
  }, [config.data])

  useEffect(() => {
    configureApiAuth(
      async () => (mode === 'FIREBASE' ? (firebase.current?.token() ?? null) : (demoSession.current?.token ?? null)),
      () => {
        void endSession()
      },
    )
  })

  async function signIn(email: string, password: string) {
    if (mode === 'FIREBASE' && firebase.current) {
      await firebase.current.signIn(email, password)
      return
    }

    try {
      const response = await api.demoLogin(email, password)
      demoSession.current = response
      saveDemoSession(response)
      setSession({ name: response.name, email: response.email })
    } catch (error) {
      throw new Error(
        error instanceof ApiError && error.status === 401
          ? 'Correo o contraseña incorrectos. Revisa los datos e inténtalo de nuevo.'
          : 'No pudimos iniciar sesión. Inténtalo de nuevo.',
      )
    }
  }

  async function endSession() {
    demoSession.current = null
    saveDemoSession(null)
    setSession(null)
    queryClient.removeQueries({ predicate: (query) => query.queryKey[0] !== 'config' })
    if (firebase.current?.auth.currentUser) await firebase.current.signOut()
  }

  const status: AuthStatus = config.isError ? 'unavailable' : config.isPending || isRestoring ? 'loading' : 'ready'

  return (
    <AuthContext.Provider
      value={{
        status,
        session,
        demoAccount: config.data?.demoAccount ?? null,
        signIn,
        signOut: () => void endSession(),
        retry: () => void config.refetch(),
      }}
    >
      {children}
    </AuthContext.Provider>
  )
}

export function useAuth() {
  const context = useContext(AuthContext)
  if (!context) throw new Error('useAuth debe usarse dentro de AuthProvider')
  return context
}
