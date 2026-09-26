import { Fragment } from 'react'
import { useNavigate } from 'react-router'
import { AnomalyTypeChip, ConfidenceMeter, SeverityIndicator, StatusPill } from '@/components/ui/Badges'
import { anomalyStatusLabel, shortActionLabel } from '@/lib/labels'
import type { Tone } from '@/lib/labels'
import type { AnomalyListItem, AnomalyStatus } from '@/lib/types'

const statusTone: Record<AnomalyStatus, Tone> = {
  OPEN: 'alert',
  INVESTIGATING: 'accent',
  RESOLVED: 'ok',
  DISMISSED: 'dismissed',
}

export function AnomalyTable({ anomalies }: { anomalies: AnomalyListItem[] }) {
  const navigate = useNavigate()
  const active = anomalies.filter((anomaly) => anomaly.anomaly)
  const dismissedByAi = anomalies.filter((anomaly) => !anomaly.anomaly)

  function renderRow(anomaly: AnomalyListItem) {
    const open = () => navigate(`/anomalies/${anomaly.id}`)
    return (
      <tr
        key={anomaly.id}
        tabIndex={0}
        onClick={open}
        onKeyDown={(event) => event.key === 'Enter' && open()}
        className={`cursor-pointer border-b border-line-soft hover:bg-surface-2 focus-visible:bg-accent-soft ${anomaly.anomaly ? '' : 'opacity-65'}`}
      >
        <td className="px-3 py-2.5 font-mono text-muted">{anomaly.rank}</td>
        <td className="px-3 py-2.5">
          <div className="font-mono text-[12.5px] font-semibold">{anomaly.meterId}</div>
          <div className="text-[11.5px] text-muted">{anomaly.meterName}</div>
        </td>
        <td className="px-3 py-2.5">
          <AnomalyTypeChip type={anomaly.type} />
        </td>
        <td className="px-3 py-2.5">
          <SeverityIndicator severity={anomaly.severity} />
        </td>
        <td className="px-3 py-2.5">
          <ConfidenceMeter value={anomaly.confidence} label={anomaly.confidenceLabel} />
        </td>
        <td className="max-w-[320px] px-3 py-2.5 text-[12px] text-muted">{anomaly.reason}</td>
        <td className="px-3 py-2.5" title={anomaly.recommendedAction}>
          <b className="font-semibold">{shortActionLabel[anomaly.type]}</b>
        </td>
        <td className="px-3 py-2.5">
          <StatusPill tone={statusTone[anomaly.status]} label={anomalyStatusLabel[anomaly.status]} />
        </td>
      </tr>
    )
  }

  return (
    <div className="overflow-x-auto rounded-xl border border-line bg-surface">
      <table className="w-full min-w-[980px] border-collapse text-[13px]">
        <thead>
          <tr className="border-b border-line text-left text-[11px] font-semibold text-muted">
            <th className="px-3 py-2.5">#</th>
            <th className="px-3 py-2.5">Medidor</th>
            <th className="px-3 py-2.5">Tipo</th>
            <th className="px-3 py-2.5">Severidad</th>
            <th className="px-3 py-2.5">Confianza</th>
            <th className="px-3 py-2.5">Razón</th>
            <th className="px-3 py-2.5">Acción</th>
            <th className="px-3 py-2.5">Estado</th>
          </tr>
        </thead>
        <tbody>
          {active.map(renderRow)}
          {dismissedByAi.length > 0 && (
            <Fragment>
              <tr>
                <td colSpan={8} className="bg-surface-2 px-3 py-1.5 text-[11px] font-semibold tracking-wide text-muted uppercase">
                  Descartadas por la IA · se muestran para transparencia
                </td>
              </tr>
              {dismissedByAi.map(renderRow)}
            </Fragment>
          )}
        </tbody>
      </table>
    </div>
  )
}
