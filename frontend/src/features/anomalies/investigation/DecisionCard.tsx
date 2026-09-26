import { AnomalyTypeChip, SeverityIndicator, StatusPill } from '@/components/ui/Badges'
import { Card } from '@/components/ui/Card'
import { formatConfidence } from '@/lib/format'
import type { AnomalyDetail } from '@/lib/types'

const factors = [
  { key: 'signal', label: 'Fuerza de la señal', help: 'Qué tan grande es el desvío' },
  { key: 'persistence', label: 'Persistencia', help: 'Cuántas horas duró' },
  { key: 'corroboration', label: 'Corroboración', help: 'Cuántas comprobaciones lo confirman' },
] as const

export function DecisionCard({ detail }: { detail: AnomalyDetail }) {
  const { summary, evidence } = detail

  return (
    <Card>
      <div className="flex flex-wrap items-center gap-2">
        <AnomalyTypeChip type={summary.type} />
        <SeverityIndicator severity={summary.severity} />
        {summary.rank > 0 && (
          <span className="ml-auto">
            <StatusPill tone={summary.rank <= 2 && summary.anomaly ? 'critical' : 'dismissed'} label={`Prioridad ${summary.rank}`} />
          </span>
        )}
      </div>
      <div className="flex items-baseline gap-2">
        <span className="font-display text-[34px] font-extrabold tabular">{formatConfidence(summary.confidence)}</span>
        <span className="text-muted">confianza · {summary.confidenceLabel}</span>
      </div>
      <ul className="grid gap-2">
        {factors.map((factor) => {
          const value = evidence.confidence[factor.key]
          return (
            <li key={factor.key} className="grid grid-cols-[120px_1fr_36px] items-center gap-2 text-xs text-muted" title={factor.help}>
              <span>{factor.label}</span>
              <span className="h-1.5 overflow-hidden rounded-full bg-line" aria-hidden>
                <span className="block h-full rounded-full bg-accent" style={{ width: `${value * 100}%` }} />
              </span>
              <span className="text-right font-mono">{formatConfidence(value)}</span>
            </li>
          )
        })}
      </ul>
      <p className="text-[11.5px] text-faint">Confianza = 0,5 + 0,45 × (0,4 señal + 0,3 persistencia + 0,3 corroboración)</p>
    </Card>
  )
}
