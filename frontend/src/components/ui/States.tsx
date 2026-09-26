import type { ReactNode } from 'react'

export function Skeleton({ className = '' }: { className?: string }) {
  return <div className={`animate-pulse rounded-md bg-line-soft ${className}`} />
}

export function LoadingBlock({ rows = 4 }: { rows?: number }) {
  return (
    <div className="grid gap-3 rounded-xl border border-line bg-surface p-4" aria-busy="true" aria-label="Cargando">
      <Skeleton className="h-4 w-1/3" />
      {Array.from({ length: rows }, (_, index) => (
        <Skeleton key={index} className="h-3.5" />
      ))}
    </div>
  )
}

interface StateMessageProps {
  eyebrow: string
  title: string
  description: string
  action?: ReactNode
}

export function StateMessage({ eyebrow, title, description, action }: StateMessageProps) {
  return (
    <div className="grid justify-items-start gap-2 rounded-xl border border-line bg-surface p-6">
      <span className="font-mono text-[11px] font-semibold uppercase tracking-wider text-accent">{eyebrow}</span>
      <h3 className="text-lg font-bold">{title}</h3>
      <p className="max-w-prose text-muted">{description}</p>
      {action && <div className="pt-1">{action}</div>}
    </div>
  )
}

export function ErrorMessage({ error, onRetry }: { error: unknown; onRetry?: () => void }) {
  const detail = error instanceof Error ? error.message : 'El servidor no respondió.'
  return (
    <StateMessage
      eyebrow="Error"
      title="No pudimos cargar esta información"
      description={`${detail} Revisa que la API esté encendida y vuelve a intentarlo.`}
      action={
        onRetry && (
          <button type="button" onClick={onRetry} className="h-9 rounded-lg border border-line px-3.5 text-[13px] font-semibold hover:bg-surface-2">
            Reintentar
          </button>
        )
      }
    />
  )
}
