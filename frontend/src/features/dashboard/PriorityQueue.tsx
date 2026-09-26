import { Link } from 'react-router'
import { AnomalyTypeChip, SeverityIndicator } from '@/components/ui/Badges'
import { Card, CardHeader } from '@/components/ui/Card'
import { anomalyStatusLabel, anomalyTypeTone, shortActionLabel, toneClasses } from '@/lib/labels'
import type { AnomalyListItem } from '@/lib/types'

export function PriorityQueue({ anomalies }: { anomalies: AnomalyListItem[] }) {
  return (
    <Card>
      <CardHeader
        title="Requiere atención"
        aside="Ordenado por prioridad de la IA"
        action={
          <Link to="/anomalies" className="text-[13px] font-semibold text-accent hover:underline">
            Ver todas →
          </Link>
        }
      />
      {anomalies.length === 0 ? (
        <p className="text-[13px] text-muted">No hay anomalías en el último análisis.</p>
      ) : (
        <ol className="grid gap-2">
          {anomalies.map((anomaly) => (
            <QueueItem key={anomaly.id} anomaly={anomaly} />
          ))}
        </ol>
      )}
    </Card>
  )
}

function QueueItem({ anomaly }: { anomaly: AnomalyListItem }) {
  const tone = toneClasses[anomalyTypeTone[anomaly.type]]
  const isPriority = anomaly.rank <= 2 && anomaly.anomaly

  return (
    <li className={`relative overflow-hidden rounded-lg border border-line bg-surface ${anomaly.anomaly ? '' : 'opacity-70'}`}>
      <span className={`absolute inset-y-0 left-0 w-1 ${tone.solid}`} aria-hidden />
      <Link to={`/anomalies/${anomaly.id}`} className="grid grid-cols-[28px_64px_1fr] items-center gap-3 py-2.5 pr-3 pl-4 hover:bg-surface-2 sm:grid-cols-[28px_64px_1fr_auto]">
        <span className="font-mono text-[13px] font-bold text-muted">#{anomaly.rank}</span>
        <span className="font-mono text-[13px] font-semibold">{anomaly.meterId}</span>
        <span className="grid gap-1">
          <span className="flex flex-wrap items-center gap-2">
            <AnomalyTypeChip type={anomaly.type} />
            <SeverityIndicator severity={anomaly.severity} />
            {anomaly.status !== 'OPEN' && <span className="text-xs text-muted">· {anomalyStatusLabel[anomaly.status]}</span>}
          </span>
          <span className="text-[12.5px] text-muted">{anomaly.reason}</span>
        </span>
        <span
          className={`hidden rounded-lg px-3 py-1.5 text-[13px] font-semibold sm:inline-flex ${isPriority ? 'bg-accent text-white' : 'border border-line text-ink'}`}
        >
          {shortActionLabel[anomaly.type]} →
        </span>
      </Link>
    </li>
  )
}
