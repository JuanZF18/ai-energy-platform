import { useState } from 'react'
import { keepPreviousData, useQuery } from '@tanstack/react-query'
import { Link, useParams } from 'react-router'
import { MeterStatusPill } from '@/components/ui/Badges'
import { Card, CardHeader } from '@/components/ui/Card'
import { PageHeader } from '@/components/ui/PageHeader'
import { ErrorMessage, LoadingBlock, StateMessage } from '@/components/ui/States'
import { ApiError, api } from '@/lib/api'
import { queryKeys } from '@/lib/queryKeys'
import type { MeterDetail, ReadingGranularity } from '@/lib/types'
import { ChartRangeControls, rangeHours } from './ChartRangeControls'
import type { ChartRange } from './ChartRangeControls'
import { toChartPoints } from './chartData'
import { ConsumptionChart } from './ConsumptionChart'
import { ElectricalCharts } from './ElectricalCharts'
import { MeterAiBanner } from './MeterAiBanner'
import { MeterKpis } from './MeterKpis'
import { MeterAnomaliesCard, MeterEventsCard } from './MeterRecords'

export default function MeterDetailPage() {
  const { meterId = '' } = useParams()
  const meter = useQuery({ queryKey: queryKeys.meter(meterId), queryFn: () => api.meter(meterId), retry: false })

  if (meter.isPending) return <LoadingBlock rows={8} />
  if (meter.error instanceof ApiError && meter.error.status === 404) return <MeterNotFound meterId={meterId} />
  if (meter.isError) return <ErrorMessage error={meter.error} onRetry={() => meter.refetch()} />

  return <MeterDetailView meter={meter.data} />
}

function MeterDetailView({ meter }: { meter: MeterDetail }) {
  const [granularity, setGranularity] = useState<ReadingGranularity>('hour')
  const [range, setRange] = useState<ChartRange>('14d')
  const mainAnomaly = meter.anomalies[0]

  const readingParams = {
    granularity,
    from: meter.lastReadingAt ? new Date(Date.parse(meter.lastReadingAt) - (rangeHours[range] - 1) * 3_600_000).toISOString() : undefined,
  }
  const readings = useQuery({
    queryKey: queryKeys.readings(meter.meterId, readingParams),
    queryFn: () => api.readings(meter.meterId, readingParams),
    placeholderData: keepPreviousData,
  })
  const investigation = useQuery({
    queryKey: queryKeys.anomaly(mainAnomaly?.id ?? 'none'),
    queryFn: () => api.anomaly(mainAnomaly!.id),
    enabled: mainAnomaly !== undefined,
  })

  const points = toChartPoints(readings.data ?? [])
  const anomalyWindow = investigation.data?.evidence.window
  const hoursPerTick = granularity === 'day' ? 1 : range === '48h' ? 12 : 24

  return (
    <>
      <PageHeader
        title={
          <span className="flex flex-wrap items-center gap-2">
            <span className="font-mono">{meter.meterId}</span> · {meter.name} <MeterStatusPill status={meter.status} />
          </span>
        }
        description={`${meter.location} · lectura horaria · ${meter.readingsCount} lecturas`}
      />
      {mainAnomaly && mainAnomaly.anomaly && <MeterAiBanner anomaly={mainAnomaly} />}
      <MeterKpis meter={meter} />
      <Card>
        <CardHeader
          title="Consumo real frente al esperado"
          action={<ChartRangeControls granularity={granularity} range={range} onGranularityChange={setGranularity} onRangeChange={setRange} />}
        />
        {readings.isError ? (
          <ErrorMessage error={readings.error} onRetry={() => readings.refetch()} />
        ) : (
          <ConsumptionChart points={points} hoursPerTick={hoursPerTick} anomalyWindow={anomalyWindow} events={meter.events} />
        )}
        <ChartLegend
          hasWindow={anomalyWindow !== undefined}
          hasSuspects={points.some((point) => point.suspectConsumption !== null)}
          hasEvents={meter.events.length > 0}
        />
      </Card>
      <Card>
        <CardHeader title="Variables eléctricas" aside={investigation.data ? 'Promedio antes → durante el cambio' : undefined} />
        <ElectricalCharts points={points} anomalyWindow={anomalyWindow} changes={investigation.data?.evidence.changedVariables} />
      </Card>
      <div className="grid gap-3 lg:grid-cols-2">
        <MeterEventsCard events={meter.events} />
        <MeterAnomaliesCard anomalies={meter.anomalies} />
      </div>
    </>
  )
}

function ChartLegend({ hasWindow, hasSuspects, hasEvents }: { hasWindow: boolean; hasSuspects: boolean; hasEvents: boolean }) {
  return (
    <div className="flex flex-wrap gap-4 text-xs text-muted">
      <LegendItem className="bg-accent" label="Consumo medido (kWh)" />
      <LegendItem className="bg-baseline" label="Consumo esperado" />
      {hasWindow && <LegendItem className="border border-critical bg-critical-soft" label="Periodo con anomalía" />}
      {hasSuspects && <LegendItem className="bg-quality" label="Lectura sospechosa" />}
      {hasEvents && <LegendItem className="h-3 w-0 rounded-none border-l-2 border-dashed border-muted" label="Evento registrado (línea punteada)" />}
    </div>
  )
}

function LegendItem({ className, label }: { className: string; label: string }) {
  return (
    <span className="flex items-center gap-1.5">
      <span className={`size-2.5 rounded-sm ${className}`} aria-hidden />
      {label}
    </span>
  )
}

function MeterNotFound({ meterId }: { meterId: string }) {
  return (
    <StateMessage
      eyebrow="No encontrado"
      title={`No existe el medidor ${meterId}`}
      description="Revisa el identificador o vuelve a la lista de medidores."
      action={
        <Link to="/meters" className="inline-flex h-9 items-center rounded-lg border border-line px-3.5 text-[13px] font-semibold hover:bg-surface-2">
          Ir a Medidores
        </Link>
      }
    />
  )
}
