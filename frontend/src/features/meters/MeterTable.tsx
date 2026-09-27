import { useNavigate } from 'react-router'
import { MeterStatusPill, SeverityIndicator } from '@/components/ui/Badges'
import { Sparkline } from '@/components/ui/Sparkline'
import { formatNumber } from '@/lib/format'
import { anomalyTypeLabel, meterStatusColor } from '@/lib/labels'
import type { MeterListItem } from '@/lib/types'
import { VariationValue } from './VariationValue'

export function MeterTable({ meters }: { meters: MeterListItem[] }) {
  const navigate = useNavigate()

  return (
    <div className="hidden overflow-x-auto rounded-xl border border-line bg-surface xl:block">
      <table className="w-full min-w-[880px] border-collapse text-[13px]">
        <thead>
          <tr className="border-b border-line text-left text-xs font-semibold text-muted">
            <th className="px-3 py-2.5">Medidor</th>
            <th className="px-3 py-2.5 text-right">Último día</th>
            <th className="px-3 py-2.5 text-right">Esperado</th>
            <th className="px-3 py-2.5 text-right">Variación diaria</th>
            <th className="px-3 py-2.5">Últimos 14 días</th>
            <th className="px-3 py-2.5">Estado</th>
            <th className="px-3 py-2.5">Anomalía</th>
          </tr>
        </thead>
        <tbody>
          {meters.map((meter) => (
            <tr
              key={meter.meterId}
              tabIndex={0}
              onClick={() => navigate(`/meters/${meter.meterId}`)}
              onKeyDown={(event) => event.key === 'Enter' && navigate(`/meters/${meter.meterId}`)}
              className="cursor-pointer border-b border-line-soft last:border-b-0 hover:bg-surface-2 focus-visible:bg-accent-soft"
            >
              <td className="px-3 py-2.5">
                <div className="font-mono text-[12.5px] font-semibold">{meter.meterId}</div>
                <div className="text-xs text-muted">
                  {meter.name} · {meter.location}
                </div>
              </td>
              <td className="px-3 py-2.5 text-right font-mono tabular">{formatNumber(meter.currentDailyKwh)} kWh</td>
              <td className="px-3 py-2.5 text-right font-mono text-muted tabular">
                {meter.baselineDailyKwh === null ? '—' : `${formatNumber(meter.baselineDailyKwh)} kWh`}
              </td>
              <td className="px-3 py-2.5 text-right">
                <VariationValue value={meter.variationPercent} />
              </td>
              <td className="px-3 py-2.5">
                <Sparkline values={meter.dailyConsumption} color={meterStatusColor[meter.status]} />
              </td>
              <td className="px-3 py-2.5">
                <MeterStatusPill status={meter.status} />
              </td>
              <td className="px-3 py-2.5">
                <AnomalyCell meter={meter} />
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}

export function AnomalyCell({ meter }: { meter: MeterListItem }) {
  if (!meter.anomaly) return <span className="text-faint">—</span>

  const isDismissed = meter.anomaly.type === 'FALSE_POSITIVE'
  return (
    <span className={`grid gap-0.5 ${isDismissed ? 'opacity-70' : ''}`}>
      <SeverityIndicator severity={meter.anomaly.severity} />
      <span className="text-xs text-muted">{isDismissed ? 'Descartada por la IA' : anomalyTypeLabel[meter.anomaly.type]}</span>
    </span>
  )
}
