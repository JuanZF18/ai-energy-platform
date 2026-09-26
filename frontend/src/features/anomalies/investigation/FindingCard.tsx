import { AiTag } from '@/components/ui/Badges'
import { Card, CardHeader } from '@/components/ui/Card'
import { formatNumber, formatSignedPercent } from '@/lib/format'
import { anomalyTypeLabel, severityLabel } from '@/lib/labels'
import type { AnomalyDetail } from '@/lib/types'
import { confirmsIssue } from './eventRelations'

export function FindingCard({ detail }: { detail: AnomalyDetail }) {
  const { summary, explanation } = detail
  const sourceLabel = explanation?.source === 'LANGUAGE_MODEL' ? 'Redactado por IA con la evidencia de abajo' : 'Redactado con plantilla a partir de la evidencia'

  return (
    <Card>
      <CardHeader title="Qué encontró la IA" action={<AiTag>{sourceLabel}</AiTag>} />
      <p className="text-[14.5px] leading-relaxed">{explanation?.summary ?? summary.reason}</p>
      <ol className="flex flex-wrap items-center gap-1.5 text-xs" aria-label="Cómo llegó la IA a esta conclusión">
        {reasoningSteps(detail).map((step, index) => (
          <li key={step.label} className="flex items-center gap-1.5">
            {index > 0 && <span className="text-faint" aria-hidden>→</span>}
            <span className="rounded-md border border-line bg-surface-2 px-2 py-1">
              {step.label}: <b>{step.value}</b>
            </span>
          </li>
        ))}
      </ol>
    </Card>
  )
}

function eventsVerdict(explainingType: string | undefined, confirmingType: string | undefined, eventCount: number) {
  if (explainingType) return `lo explica ${explainingType}`
  if (confirmingType) return `lo confirma ${confirmingType}`
  return eventCount > 0 ? 'ninguno lo explica' : 'no hay eventos'
}

function reasoningSteps({ summary, evidence }: AnomalyDetail) {
  const detection = evidence.dataQuality
    ? `${evidence.dataQuality.suspectReadings} lecturas sospechosas`
    : `${formatSignedPercent(evidence.consumption.meanHourlyDeviationPercent)} durante ${evidence.window.hours} h`

  const strongestChange = [...evidence.changedVariables]
    .filter((change) => change.variable !== 'Consumo')
    .sort((left, right) => Math.abs(right.changePercent) - Math.abs(left.changePercent))[0]

  const explainingEvent = evidence.events.find((event) => event.explainsChange)
  const confirmingEvent = evidence.events.find((event) => confirmsIssue(evidence, event.type))

  return [
    { label: 'Detección', value: detection },
    {
      label: 'Correlación',
      value: strongestChange
        ? `${strongestChange.variable} ${formatNumber(strongestChange.before, 2)} → ${formatNumber(strongestChange.after, 2)}`
        : 'sin cambios eléctricos',
    },
    { label: 'Eventos', value: eventsVerdict(explainingEvent?.type, confirmingEvent?.type, evidence.events.length) },
    { label: 'Clasificación', value: `${anomalyTypeLabel[summary.type]} · ${severityLabel[summary.severity]}` },
  ]
}
