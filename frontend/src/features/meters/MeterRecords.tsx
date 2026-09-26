import { Link } from 'react-router'
import { AnomalyTypeChip, SeverityIndicator } from '@/components/ui/Badges'
import { Card, CardHeader } from '@/components/ui/Card'
import { formatPlantDateTime } from '@/lib/format'
import { anomalyStatusLabel, eventTypeLabel } from '@/lib/labels'
import type { AnomalyListItem, MeterEvent } from '@/lib/types'

export function MeterEventsCard({ events }: { events: MeterEvent[] }) {
  return (
    <Card>
      <CardHeader title="Eventos del medidor" />
      {events.length === 0 ? (
        <p className="text-[13px] text-muted">No hay eventos operativos registrados.</p>
      ) : (
        <ul className="grid gap-2">
          {events.map((event) => (
            <li key={event.timestamp} className="grid grid-cols-[92px_auto_1fr] items-baseline gap-2 text-[13px]">
              <span className="font-mono text-[11.5px] text-muted">{formatPlantDateTime(event.timestamp)}</span>
              <span className="rounded bg-dismissed-soft px-1.5 text-[11px] font-semibold text-dismissed">{eventTypeLabel(event.type)}</span>
              <span>{event.description}</span>
            </li>
          ))}
        </ul>
      )}
    </Card>
  )
}

export function MeterAnomaliesCard({ anomalies }: { anomalies: AnomalyListItem[] }) {
  return (
    <Card>
      <CardHeader title="Anomalías de este medidor" />
      {anomalies.length === 0 ? (
        <p className="text-[13px] text-muted">El último análisis no encontró casos en este medidor.</p>
      ) : (
        <ul className="grid gap-2">
          {anomalies.map((anomaly) => (
            <li key={anomaly.id}>
              <Link to={`/anomalies/${anomaly.id}`} className="flex flex-wrap items-center gap-2 rounded-lg px-2 py-1.5 text-[13px] hover:bg-surface-2">
                <AnomalyTypeChip type={anomaly.type} />
                <SeverityIndicator severity={anomaly.severity} />
                <span className="text-muted">{anomalyStatusLabel[anomaly.status]}</span>
                <span className="ml-auto text-accent">Ver →</span>
              </Link>
            </li>
          ))}
        </ul>
      )}
    </Card>
  )
}
