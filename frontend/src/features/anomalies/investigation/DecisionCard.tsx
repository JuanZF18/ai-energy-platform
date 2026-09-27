import { AnomalyTypeChip, SeverityIndicator, StatusPill } from '@/components/ui/Badges'
import { Card } from '@/components/ui/Card'
import { formatConfidence } from '@/lib/format'
import type { AnomalyDetail } from '@/lib/types'

const factors = [
  { key: 'signal', label: 'Tamaño del cambio', help: 'Qué tan lejos está del consumo esperado' },
  { key: 'persistence', label: 'Duración', help: 'Cuántas horas seguidas se mantuvo' },
  { key: 'corroboration', label: 'Confirmación', help: 'Cuántas comprobaciones independientes lo respaldan' },
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
        <span className="text-muted">de confianza · {summary.confidenceLabel.toLowerCase()}</span>
      </div>
      <p className="text-xs text-muted">Qué tan segura está la IA de su conclusión. Se calcula con tres factores:</p>
      <ul className="grid gap-2">
        {factors.map((factor) => {
          const value = evidence.confidence[factor.key]
          return (
            <li key={factor.key} className="grid grid-cols-[1fr_72px_40px] items-center gap-2 text-xs">
              <span className="grid">
                <span className="font-semibold">{factor.label}</span>
                <span className="text-xs text-muted">{factor.help}</span>
              </span>
              <span className="h-1.5 overflow-hidden rounded-full bg-line" aria-hidden>
                <span className="block h-full rounded-full bg-accent" style={{ width: `${value * 100}%` }} />
              </span>
              <span className="text-right font-mono text-muted">{formatConfidence(value)}</span>
            </li>
          )
        })}
      </ul>
      <p className="text-xs text-muted">Peso de cada factor: tamaño 40%, duración 30%, confirmación 30%. La confianza va de 50% a 95%: la IA nunca afirma certeza total, así que con los tres factores al máximo marca 95%.</p>
    </Card>
  )
}
