import { Link } from 'react-router'
import { MeterStatusPill } from '@/components/ui/Badges'
import { Sparkline } from '@/components/ui/Sparkline'
import { formatNumber } from '@/lib/format'
import { meterStatusColor } from '@/lib/labels'
import type { MeterListItem } from '@/lib/types'
import { AnomalyCell } from './MeterTable'
import { VariationValue } from './VariationValue'

export function MeterCardList({ meters }: { meters: MeterListItem[] }) {
  return (
    <ul className="grid gap-2 sm:grid-cols-2 xl:hidden">
      {meters.map((meter) => (
        <li key={meter.meterId}>
          <Link to={`/meters/${meter.meterId}`} className="grid gap-2 rounded-xl border border-line bg-surface p-3 hover:bg-surface-2">
            <span className="flex items-start justify-between gap-3">
              <span className="grid">
                <span className="font-mono text-[13px] font-semibold">{meter.meterId}</span>
                <span className="text-xs text-muted">
                  {meter.name} · {meter.location}
                </span>
              </span>
              <MeterStatusPill status={meter.status} />
            </span>
            <span className="flex items-end justify-between gap-3">
              <span className="grid gap-0.5 text-xs text-muted">
                <span>
                  Último día <b className="font-mono font-semibold text-ink tabular">{formatNumber(meter.currentDailyKwh)} kWh</b>
                </span>
                <span>
                  Esperado <span className="font-mono tabular">{meter.baselineDailyKwh === null ? '—' : `${formatNumber(meter.baselineDailyKwh)} kWh`}</span> ·{' '}
                  <VariationValue value={meter.variationPercent} />
                </span>
              </span>
              <Sparkline values={meter.dailyConsumption} color={meterStatusColor[meter.status]} width={96} />
            </span>
            {meter.anomaly && <AnomalyCell meter={meter} />}
          </Link>
        </li>
      ))}
    </ul>
  )
}
