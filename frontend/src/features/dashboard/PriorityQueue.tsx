import { AnomalyTypeChip, SeverityIndicator } from '@/components/ui/Badges'
import { ButtonLink } from '@/components/ui/Button'
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
          <ButtonLink to="/anomalies" variant="ghost" className="-my-1.5">
            Ver todas →
          </ButtonLink>
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
        <div key={anomaly.id} className="grid gap-2 rounded-lg bg-surface-2 px-3 py-2.5 text-[13px] text-muted sm:grid-cols-[1fr_auto] sm:items-center">
          <span className="grid gap-0.5">
            <span className="flex flex-wrap items-center gap-x-2">
              <span className="font-mono font-semibold text-ink">{anomaly.meterId}</span>
              <span className="font-semibold">Falso positivo: la IA lo descartó, no requiere atención</span>
            </span>
            <span>{anomaly.reason}</span>
          </span>
          <ButtonLink to={`/anomalies/${anomaly.id}`} variant="secondary" className="sm:w-44">
            Ver por qué →
          </ButtonLink>
        </div>
      ))}
    </Card>
  )
}

function QueueItem({ anomaly }: { anomaly: AnomalyListItem }) {
  const tone = toneClasses[anomalyTypeTone[anomaly.type]]
  const isPriority = anomaly.rank <= 2

  return (
    <li className="relative grid grid-cols-[28px_1fr] items-center gap-x-3 gap-y-2 overflow-hidden rounded-lg border border-line bg-surface py-2.5 pr-3 pl-4 sm:grid-cols-[28px_64px_1fr_auto]">
      <span className={`absolute inset-y-0 left-0 w-1 ${tone.solid}`} aria-hidden />
      <span className="font-mono text-[13px] font-bold text-muted">#{anomaly.rank}</span>
      <span className="hidden font-mono text-[13px] font-semibold sm:block">{anomaly.meterId}</span>
      <span className="grid gap-1">
        <span className="flex flex-wrap items-center gap-2">
          <span className="font-mono text-[13px] font-semibold sm:hidden">{anomaly.meterId}</span>
          <AnomalyTypeChip type={anomaly.type} />
          <SeverityIndicator severity={anomaly.severity} />
          {anomaly.status !== 'OPEN' && <span className="text-xs text-muted">· {anomalyStatusLabel[anomaly.status]}</span>}
        </span>
        <span className="text-[13px] text-muted">{anomaly.reason}</span>
      </span>
      <ButtonLink
        to={`/anomalies/${anomaly.id}`}
        variant={isPriority ? 'primary' : 'secondary'}
        className="col-start-2 sm:col-start-auto sm:w-44"
      >
        {shortActionLabel[anomaly.type]} →
      </ButtonLink>
    </li>
  )
}
