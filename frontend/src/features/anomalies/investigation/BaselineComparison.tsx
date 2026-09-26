import { useQuery } from '@tanstack/react-query'
import { Card, CardHeader } from '@/components/ui/Card'
import { ErrorMessage, Skeleton } from '@/components/ui/States'
import { toChartPoints } from '@/features/meters/chartData'
import { ConsumptionChart } from '@/features/meters/ConsumptionChart'
import { ElectricalCharts } from '@/features/meters/ElectricalCharts'
import { api } from '@/lib/api'
import { formatKwh, formatSignedPercent } from '@/lib/format'
import { queryKeys } from '@/lib/queryKeys'
import type { AnomalyDetail, MeterEvent } from '@/lib/types'

const hour = 3_600_000
const contextBeforeHours = 72
const contextAfterHours = 24

export function BaselineComparison({ detail }: { detail: AnomalyDetail }) {
  const { evidence, summary } = detail
  const params = {
    granularity: 'hour' as const,
    from: new Date(Date.parse(evidence.window.start) - contextBeforeHours * hour).toISOString(),
    to: evidence.window.isOngoing ? undefined : new Date(Date.parse(evidence.window.end) + contextAfterHours * hour).toISOString(),
  }
  const readings = useQuery({ queryKey: queryKeys.readings(summary.meterId, params), queryFn: () => api.readings(summary.meterId, params) })

  const points = toChartPoints(readings.data ?? [])
  const events: MeterEvent[] = evidence.events.map((event) => ({ ...event, meterId: summary.meterId }))
  const isDataQuality = evidence.dataQuality !== null

  return (
    <Card>
      <CardHeader
        title="Comparación contra el baseline"
        aside={`Baseline ${formatKwh(evidence.consumption.baselineDailyKwh)}/día · observado ${formatKwh(evidence.consumption.observedDailyKwh)} · ${formatSignedPercent(evidence.consumption.variationPercent)}`}
      />
      {readings.isPending && <Skeleton className="h-[220px]" />}
      {readings.isError && <ErrorMessage error={readings.error} onRetry={() => readings.refetch()} />}
      {readings.data && (
        <>
          <ConsumptionChart points={points} hoursPerTick={24} anomalyWindow={isDataQuality ? undefined : evidence.window} events={events} height={220} />
          {isDataQuality && <ElectricalCharts points={points} />}
        </>
      )}
    </Card>
  )
}
