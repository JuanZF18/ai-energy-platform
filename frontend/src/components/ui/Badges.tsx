import { anomalyTypeLabel, anomalyTypeTone, meterStatusLabel, meterStatusTone, severityLabel, severityLevel, toneClasses } from '@/lib/labels'
import type { Tone } from '@/lib/labels'
import type { AnomalyType, MeterStatus, Severity } from '@/lib/types'
import { formatConfidence } from '@/lib/format'

export function StatusPill({ label, tone }: { label: string; tone: Tone }) {
  const colors = toneClasses[tone]
  return (
    <span className={`inline-flex items-center gap-1.5 whitespace-nowrap rounded-full px-2.5 py-0.5 text-xs font-semibold ${colors.soft} ${colors.text}`}>
      <span className={`size-1.5 rounded-full ${colors.solid}`} />
      {label}
    </span>
  )
}

export function MeterStatusPill({ status }: { status: MeterStatus }) {
  return <StatusPill label={meterStatusLabel[status]} tone={meterStatusTone[status]} />
}

export function AnomalyTypeChip({ type }: { type: AnomalyType }) {
  const colors = toneClasses[anomalyTypeTone[type]]
  return (
    <span className={`inline-flex whitespace-nowrap rounded-md px-2 py-0.5 text-xs font-semibold ${colors.soft} ${colors.text}`}>
      {anomalyTypeLabel[type]}
    </span>
  )
}

const severityColor: Record<Severity, string> = {
  HIGH: 'bg-critical',
  MEDIUM: 'bg-alert',
  LOW: 'bg-dismissed',
}

export function SeverityIndicator({ severity }: { severity: Severity }) {
  const level = severityLevel[severity]
  return (
    <span className="inline-flex items-center gap-1.5 text-xs font-semibold" aria-label={`Severidad ${severityLabel[severity]}`}>
      <span className="inline-flex gap-0.5" aria-hidden>
        {[1, 2, 3].map((bar) => (
          <span key={bar} className={`h-3 w-1 rounded-sm ${bar <= level ? severityColor[severity] : 'bg-line'}`} />
        ))}
      </span>
      {severityLabel[severity]}
    </span>
  )
}

export function ConfidenceMeter({ value, label }: { value: number; label: string }) {
  return (
    <span className="inline-flex items-center gap-2 text-xs">
      <span className="h-1.5 w-14 overflow-hidden rounded-full bg-line" aria-hidden>
        <span className="block h-full rounded-full bg-accent" style={{ width: `${Math.round(value * 100)}%` }} />
      </span>
      <b className="font-semibold">{label}</b>
      <span className="font-mono text-muted tabular">{formatConfidence(value)}</span>
    </span>
  )
}

export function AiTag({ children = 'IA' }: { children?: string }) {
  return (
    <span className="inline-flex items-center rounded bg-accent-soft px-1.5 py-0.5 font-mono text-[10.5px] font-semibold uppercase tracking-wide text-accent">
      {children}
    </span>
  )
}
