import { Link } from 'react-router'
import { AiTag } from '@/components/ui/Badges'
import { formatConfidence } from '@/lib/format'
import { anomalyTypeLabel, anomalyTypeTone, severityLabel, toneClasses } from '@/lib/labels'
import type { AnomalyListItem } from '@/lib/types'

export function MeterAiBanner({ anomaly }: { anomaly: AnomalyListItem }) {
  const tone = toneClasses[anomalyTypeTone[anomaly.type]]

  return (
    <div className={`flex flex-wrap items-center gap-3 rounded-xl border px-4 py-3 ${tone.border} ${tone.soft}`}>
      <span className={`grid size-8 shrink-0 place-items-center rounded-lg font-display font-extrabold text-white ${tone.solid}`} aria-hidden>
        !
      </span>
      <p className="min-w-[260px] flex-1 text-[13.5px]">
        <AiTag />{' '}
        <b>
          {anomalyTypeLabel[anomaly.type]} · severidad {severityLabel[anomaly.severity].toLowerCase()} · {formatConfidence(anomaly.confidence)} de confianza.
        </b>{' '}
        {anomaly.reason}
      </p>
      <Link to={`/anomalies/${anomaly.id}`} className="inline-flex h-9 items-center rounded-lg bg-accent px-3.5 text-[13px] font-semibold text-white hover:bg-accent-strong">
        Ver investigación →
      </Link>
    </div>
  )
}
