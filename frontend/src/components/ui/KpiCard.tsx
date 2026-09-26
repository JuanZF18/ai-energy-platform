import type { ReactNode } from 'react'

interface KpiCardProps {
  label: string
  value: ReactNode
  unit?: string
  hint?: ReactNode
  highlighted?: boolean
  children?: ReactNode
}

export function KpiCard({ label, value, unit, hint, highlighted = false, children }: KpiCardProps) {
  return (
    <div
      className={`grid content-start gap-1 rounded-xl border bg-surface px-4 py-3 ${highlighted ? 'border-critical shadow-[inset_3px_0_0_var(--color-critical)]' : 'border-line'}`}
    >
      <span className="text-xs font-medium text-muted">{label}</span>
      <span className="font-display text-[26px] leading-tight font-bold tabular">
        {value}
        {unit && <small className="ml-1 text-[13px] font-semibold text-muted">{unit}</small>}
      </span>
      {children}
      {hint && <span className="text-xs text-muted">{hint}</span>}
    </div>
  )
}
