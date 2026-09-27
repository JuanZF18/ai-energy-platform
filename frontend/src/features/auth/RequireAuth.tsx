import type { ReactNode } from 'react'
import { Navigate, useLocation } from 'react-router'
import { Button } from '@/components/ui/Button'
import { StateMessage } from '@/components/ui/States'
import { useAuth } from './AuthContext'

export function RequireAuth({ children }: { children: ReactNode }) {
  const { status, session, retry } = useAuth()
  const location = useLocation()

  if (status === 'loading') return <AuthScreen>Verificando tu sesión…</AuthScreen>
  if (status === 'unavailable') return <ServerUnavailable onRetry={retry} />
  if (!session) return <Navigate to="/login" replace state={{ from: location.pathname }} />

  return children
}

export function AuthScreen({ children }: { children: ReactNode }) {
  return <div className="grid min-h-full place-items-center bg-canvas p-6 text-[13px] text-muted">{children}</div>
}

export function ServerUnavailable({ onRetry }: { onRetry: () => void }) {
  return (
    <div className="grid min-h-full place-items-center bg-canvas p-6">
      <div className="w-full max-w-[460px]">
        <StateMessage
          eyebrow="Sin conexión"
          title="No pudimos conectar con el servidor"
          description="Revisa que la API esté encendida y vuelve a intentarlo."
          action={<Button onClick={onRetry}>Reintentar</Button>}
        />
      </div>
    </div>
  )
}
