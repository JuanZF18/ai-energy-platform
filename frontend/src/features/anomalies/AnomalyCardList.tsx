import { Link } from 'react-router'
import { AnomalyTypeChip, ConfidenceMeter, SeverityIndicator, StatusPill } from '@/components/ui/Badges'
import { anomalyStatusLabel, anomalyStatusTone, shortActionLabel } from '@/lib/labels'
import type { AnomalyListItem } from '@/lib/types'

export function AnomalyCardList({ anomalies }: { anomalies: AnomalyListItem[] }) {
  const active = anomalies.filter((anomaly) => anomaly.anomaly)
  const dismissedByAi = anomalies.filter((anomaly) => !anomaly.anomaly)

  return (
    <div className="grid gap-2 sm:grid-cols-2 xl:hidden">
      {active.map((anomaly) => (
        <AnomalyCard key={anomaly.id} anomaly={anomaly} />
      ))}
      {dismissedByAi.length > 0 && (
        <p className="pt-2 text-xs font-semibold tracking-wide text-muted uppercase sm:col-span-2">Descartadas por la IA · se muestran para transparencia</p>
      )}
      {dismissedByAi.map((anomaly) => (
        <AnomalyCard key={anomaly.id} anomaly={anomaly} />
      ))}
    </div>
  )
}

function AnomalyCard({ anomaly }: { anomaly: AnomalyListItem }) {
  return (
    <Link
      to={`/anomalies/${anomaly.id}`}
      className={`grid gap-2 rounded-xl border border-line bg-surface p-3 hover:bg-surface-2 ${anomaly.anomaly ? '' : 'opacity-70'}`}
    >
      <span className="flex items-start justify-between gap-3">
        <span className="grid">
          <span className="font-mono text-[13px] font-semibold">
            <span className="text-muted">#{anomaly.rank}</span> {anomaly.meterId}
          </span>
          <span className="text-xs text-muted">{anomaly.meterName}</span>
        </span>
        <StatusPill tone={anomalyStatusTone[anomaly.status]} label={anomalyStatusLabel[anomaly.status]} />
      </span>
      <span className="flex flex-wrap items-center gap-2">
        <AnomalyTypeChip type={anomaly.type} />
        <SeverityIndicator severity={anomaly.severity} />
        <ConfidenceMeter value={anomaly.confidence} label={anomaly.confidenceLabel} />
      </span>
      <span className="text-[13px] text-muted">{anomaly.reason}</span>
      <span className="text-[13px]">
        Acción: <b className="font-semibold">{shortActionLabel[anomaly.type]}</b>
      </span>
    </Link>
  )
}
