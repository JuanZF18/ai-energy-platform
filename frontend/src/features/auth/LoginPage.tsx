import { useState } from 'react'
import type { FormEvent } from 'react'
import { Navigate, useLocation, useNavigate } from 'react-router'
import { Brand } from '@/components/BrandMark'
import { Button } from '@/components/ui/Button'
import { useAuth } from './AuthContext'
import { demoAccount } from './demoAccount'

const highlights = [
  { verb: 'Detecta', text: 'picos, cambios persistentes y lecturas eléctricas inconsistentes' },
  { verb: 'Explica', text: 'con cifras antes y después y con los eventos relacionados' },
  { verb: 'Prioriza', text: 'por severidad y confianza, y descarta los falsos positivos' },
]

export function LoginPage() {
  const { session, signIn } = useAuth()
  const navigate = useNavigate()
  const location = useLocation()
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState<string | null>(null)
  const [isSubmitting, setIsSubmitting] = useState(false)
  const destination = (location.state as { from?: string } | null)?.from ?? '/'

  if (session) return <Navigate to={destination} replace />

  async function handleSubmit(event: FormEvent) {
    event.preventDefault()
    setIsSubmitting(true)
    setError(null)
    try {
      await signIn(email, password)
      navigate(destination, { replace: true })
    } catch (signInError) {
      setError(signInError instanceof Error ? signInError.message : 'No pudimos iniciar sesión.')
    } finally {
      setIsSubmitting(false)
    }
  }

  function fillDemoAccount() {
    setEmail(demoAccount.email)
    setPassword(demoAccount.password)
    setError(null)
  }

  return (
    <div className="grid min-h-full lg:grid-cols-[1.1fr_1fr]">
      <aside className="hidden flex-col gap-6 bg-nav p-10 text-white lg:flex">
        <Brand />
        <h1 className="mt-10 max-w-[18ch] text-[34px] leading-tight font-extrabold">Sabe qué medidor atender primero y por qué.</h1>
        <p className="max-w-[44ch] text-nav-ink">
          La IA revisa cada lectura contra el comportamiento esperado del medidor, cruza los eventos operativos y te entrega una acción con su evidencia.
        </p>
        <ul className="grid gap-3 text-[13.5px] text-nav-ink">
          {highlights.map((item) => (
            <li key={item.verb}>
              <b className="text-white">{item.verb}</b> {item.text}
            </li>
          ))}
        </ul>
      </aside>

      <main className="grid place-items-center bg-surface p-6">
        <form onSubmit={handleSubmit} className="grid w-full max-w-[360px] gap-4" noValidate>
          <div className="lg:hidden">
            <Brand tone="dark" />
          </div>
          <div>
            <h2 className="text-2xl font-bold">Iniciar sesión</h2>
            <p className="text-[13px] text-muted">Accede al panel de operación</p>
          </div>
          <label className="grid gap-1.5 text-[13px] font-semibold">
            Correo
            <input
              id="login-email"
              type="email"
              autoComplete="username"
              value={email}
              onChange={(event) => setEmail(event.target.value)}
              className="h-10 rounded-lg border border-line bg-surface-2 px-3 font-normal"
              required
            />
          </label>
          <label className="grid gap-1.5 text-[13px] font-semibold">
            Contraseña
            <input
              id="login-password"
              type="password"
              autoComplete="current-password"
              value={password}
              onChange={(event) => setPassword(event.target.value)}
              className="h-10 rounded-lg border border-line bg-surface-2 px-3 font-normal"
              required
            />
          </label>
          {error && (
            <p role="alert" className="rounded-lg bg-critical-soft px-3 py-2 text-[13px] text-critical">
              {error}
            </p>
          )}
          <Button type="submit" size="lg" disabled={isSubmitting}>
            {isSubmitting ? 'Entrando…' : 'Entrar'}
          </Button>
          <div className="grid gap-2 rounded-lg border border-dashed border-accent bg-accent-soft px-3 py-2.5 text-[13px]">
            <span>
              <b>Cuenta de demo:</b> <span className="font-mono">{demoAccount.email}</span> · <span className="font-mono">{demoAccount.password}</span>
            </span>
            <Button variant="ghost" size="sm" className="justify-self-start" onClick={fillDemoAccount}>
              Usar cuenta demo
            </Button>
          </div>
        </form>
      </main>
    </div>
  )
}
