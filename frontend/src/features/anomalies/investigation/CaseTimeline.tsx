import { Card, CardHeader } from '@/components/ui/Card'
import { formatLocalDateTime, formatPlantDateTime } from '@/lib/format'
import { anomalyStatusLabel, eventTypeLabel } from '@/lib/labels'
import type { AnomalyDetail } from '@/lib/types'

interface TimelineEntry {
  time: string
  text: string
  note?: string | null
}

export function CaseTimeline({ detail }: { detail: AnomalyDetail }) {
  const { evidence, summary, history } = detail
  const entries: TimelineEntry[] = [
    { time: formatPlantDateTime(evidence.window.start), text: summary.type === 'DATA_QUALITY' ? 'Primera lectura inconsistente' : 'Empieza el cambio de consumo' },
    ...evidence.events.map((event) => ({ time: formatPlantDateTime(event.timestamp), text: `${eventTypeLabel(event.type)} registrado` })),
    { time: formatLocalDateTime(summary.detectedAt), text: 'Detectada por el análisis' },
    ...history.map((change) => ({
      time: formatLocalDateTime(change.changedAt),
      text: `${anomalyStatusLabel[change.from]} → ${anomalyStatusLabel[change.to]} · ${change.changedBy}`,
      note: change.note,
    })),
  ]

  return (
    <Card>
      <CardHeader title="Línea de tiempo" />
      <ol className="grid gap-2.5 text-[12.5px]">
        {entries.map((entry, index) => (
          <li key={`${entry.time}-${index}`} className="grid grid-cols-[84px_1fr] gap-2">
            <time className="font-mono text-[11px] text-muted">{entry.time}</time>
            <span>
              {entry.text}
              {entry.note && <span className="block text-muted">"{entry.note}"</span>}
            </span>
          </li>
        ))}
      </ol>
    </Card>
  )
}
