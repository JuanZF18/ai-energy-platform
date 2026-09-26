import { MeterStatusPill } from '@/components/ui/Badges'
import { KpiCard } from '@/components/ui/KpiCard'
import { formatKwh, formatNumber, formatPlantDay } from '@/lib/format'
import type { MeterDetail } from '@/lib/types'
import { VariationValue } from './VariationValue'

export function MeterKpis({ meter }: { meter: MeterDetail }) {
  const difference = meter.baselineDailyKwh === null ? null : meter.currentDailyKwh - meter.baselineDailyKwh
  const activeAnomalies = meter.anomalies.filter((anomaly) => anomaly.status === 'OPEN' || anomaly.status === 'INVESTIGATING').length

  return (
    <div className="grid grid-cols-2 gap-2.5 lg:grid-cols-4">
      <KpiCard label="Consumo del último día" value={formatNumber(meter.currentDailyKwh)} unit="kWh" hint={`Último día con datos (${formatPlantDay(meter.lastReadingAt)})`} />
      <KpiCard
        label="Consumo esperado"
        value={meter.baselineDailyKwh === null ? '—' : formatNumber(meter.baselineDailyKwh)}
        unit={meter.baselineDailyKwh === null ? undefined : 'kWh/día'}
        hint={meter.baselineDailyKwh === null ? 'Se calcula al ejecutar el análisis' : 'Según el comportamiento normal del medidor'}
      />
      <KpiCard
        label="Variación diaria"
        value={<VariationValue value={meter.variationPercent} />}
        highlighted={meter.status === 'CRITICAL'}
        hint={difference === null ? '—' : `${difference >= 0 ? '+' : '−'}${formatKwh(Math.abs(difference))} frente a lo esperado`}
      />
      <KpiCard
        label="Estado"
        value={<MeterStatusPill status={meter.status} />}
        hint={meter.status === 'NOT_ANALYZED' ? 'Sin analizar' : `${activeAnomalies} anomalía${activeAnomalies === 1 ? '' : 's'} activa${activeAnomalies === 1 ? '' : 's'}`}
      />
    </div>
  )
}
