import { Link } from 'react-router'
import { AnomalyTypeChip, SeverityIndicator } from '@/components/ui/Badges'
import { Card, CardHeader } from '@/components/ui/Card'
import { anomalyStatusLabel, anomalyTypeTone, shortActionLabel, toneClasses } from '@/lib/labels'
import type { AnomalyListItem } from '@/lib/types'

export function PriorityQueue({ anomalies }: { anomalies: AnomalyListItem[] }) {
  const toAttend = anomalies.filter((anomaly) => anomaly.anomaly)
  const dismissed = anomalies.filter((anomaly) => !anomaly.anomaly)

  return (
    <Card>
      <CardHeader
        title="Requiere atención"
        aside="De la más a la menos urgente"
        action={
          <Link to="/anomalies" className="text-[13px] font-semibold text-accent hover:underline">
            Ver todas →
          </Link>
        }
      />
      {toAttend.length === 0 ? (
        <p className="text-[13px] text-muted">No hay anomalías por atender en el último análisis.</p>
      ) : (
        <ol className="grid gap-2">
          {toAttend.map((anomaly) => (
            <QueueItem key={anomaly.id} anomaly={anomaly} />
          ))}
        </ol>
      )}
      {dismissed.map((anomaly) => (
        <Link
          key={anomaly.id}
          to={`/anomalies/${anomaly.id}`}
          className="grid gap-0.5 rounded-lg bg-surface-2 px-3 py-2 text-[12.5px] text-muted hover:text-ink"
        >
          <span className="flex flex-wrap items-center gap-x-2">
            <span className="font-mono font-semibold text-ink">{anomaly.meterId}</span>
            <span className="font-semibold">Falso positivo: la IA lo descartó, no requiere atención</span>
            <span className="ml-auto font-semibold text-accent">Ver por qué →</span>
          </span>
          <span>{anomaly.reason}</span>
        </Link>
      ))}
    </Card>
  )
}

function QueueItem({ anomaly }: { anomaly: AnomalyListItem }) {
  const tone = toneClasses[anomalyTypeTone[anomaly.type]]
  const isPriority = anomaly.rank <= 2

  return (
    <li className="relative overflow-hidden rounded-lg border border-line bg-surface">
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
