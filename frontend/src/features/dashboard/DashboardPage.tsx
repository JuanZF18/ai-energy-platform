import { useQuery } from '@tanstack/react-query'
import { PageHeader } from '@/components/ui/PageHeader'
import { ErrorMessage, LoadingBlock, StateMessage } from '@/components/ui/States'
import { RunAnalysisButton } from '@/features/analysis/RunAnalysisButton'
import { api } from '@/lib/api'
import { formatPlantDay } from '@/lib/format'
import { queryKeys } from '@/lib/queryKeys'
import { FleetConsumptionChart } from './FleetConsumptionChart'
import { FleetStatus } from './FleetStatus'
import { KpiRow } from './KpiRow'
import { PriorityQueue } from './PriorityQueue'

function greeting() {
  const hour = new Date().getHours()
  return hour < 12 ? 'Buenos días' : hour < 19 ? 'Buenas tardes' : 'Buenas noches'
}

export default function DashboardPage() {
  const dashboard = useQuery({ queryKey: queryKeys.dashboard, queryFn: api.dashboard })
  const queue = useQuery({ queryKey: queryKeys.anomalies({ limit: 5 }), queryFn: () => api.anomalies({ limit: 5 }) })
  const meters = useQuery({ queryKey: queryKeys.meters({}), queryFn: () => api.meters() })

  if (dashboard.isPending) return <LoadingBlock rows={8} />
  if (dashboard.isError) return <ErrorMessage error={dashboard.error} onRetry={() => dashboard.refetch()} />

  const summary = dashboard.data
  const hasAnalysis = summary.meters.notAnalyzed < summary.meters.total

  return (
    <>
      <PageHeader
        title={greeting()}
        description={`Período analizado: ${formatPlantDay(summary.consumption.periodStart)} – ${formatPlantDay(summary.consumption.periodEnd)} · ${summary.meters.total} medidores · lectura horaria`}
      />
      {!hasAnalysis && (
        <StateMessage
          eyebrow="Sin análisis"
          title="Aún no hay un análisis de anomalías"
          description="Los datos ya están cargados. Ejecuta el análisis para que la IA revise las 4.032 lecturas, detecte los casos y los priorice."
          action={<RunAnalysisButton />}
        />
      )}
      <KpiRow
        summary={summary}
        hasAnalysis={hasAnalysis}
        priorityMeterIds={queue.data
          ?.filter((anomaly) => anomaly.severity === 'HIGH' && (anomaly.type === 'REAL_ANOMALY' || anomaly.type === 'DATA_QUALITY'))
          .map((anomaly) => anomaly.meterId)}
      />
      <div className="grid gap-3 xl:grid-cols-[1.55fr_1fr]">
        {hasAnalysis ? <PriorityQueue anomalies={queue.data ?? []} /> : <LoadingOrEmptyQueue />}
        {meters.data && <FleetStatus meters={meters.data} />}
      </div>
      <FleetConsumptionChart summary={summary} />
    </>
  )
}

function LoadingOrEmptyQueue() {
  return (
    <div className="grid content-center rounded-xl border border-dashed border-line bg-surface p-6 text-center text-[13px] text-muted">
      La cola de prioridad aparece después del primer análisis.
    </div>
  )
}
