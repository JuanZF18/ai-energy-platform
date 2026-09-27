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
      className={`grid min-w-0 content-start gap-1 rounded-xl border bg-surface px-3 py-3 sm:px-4 ${highlighted ? 'border-critical shadow-[inset_3px_0_0_var(--color-critical)]' : 'border-line'}`}
    >
      <span className="text-xs font-medium text-muted">{label}</span>
      <span className="flex flex-wrap items-baseline gap-x-1 font-display text-[22px] leading-tight font-bold tabular sm:text-[26px]">
        {value}
        {unit && <small className="text-[13px] font-semibold text-muted">{unit}</small>}
      </span>
      {children}
      {hint && <span className="text-xs text-muted">{hint}</span>}
    </div>
  )
}
