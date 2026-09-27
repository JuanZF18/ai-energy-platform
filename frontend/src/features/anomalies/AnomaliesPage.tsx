import { useState } from 'react'
import { useQuery } from '@tanstack/react-query'
import { PageHeader } from '@/components/ui/PageHeader'
import { SegmentedControl } from '@/components/ui/SegmentedControl'
import { ErrorMessage, LoadingBlock, StateMessage } from '@/components/ui/States'
import { RunAnalysisButton } from '@/features/analysis/RunAnalysisButton'
import { api } from '@/lib/api'
import { countLabel, formatLocalDateTime } from '@/lib/format'
import { anomalyStatusLabel } from '@/lib/labels'
import { queryKeys } from '@/lib/queryKeys'
import type { AnomalyStatus, AnomalyType } from '@/lib/types'
import { AnomalyCardList } from './AnomalyCardList'
import { AnomalyTable } from './AnomalyTable'

type TypeFilter = 'ALL' | AnomalyType

const typeOptions: { value: TypeFilter; label: string }[] = [
  { value: 'ALL', label: 'Todas' },
  { value: 'REAL_ANOMALY', label: 'Anomalía real' },
  { value: 'DATA_QUALITY', label: 'Calidad de datos' },
  { value: 'EXPLAINABLE_ANOMALY', label: 'Explicable' },
  { value: 'FALSE_POSITIVE', label: 'Falso positivo' },
]

export default function AnomaliesPage() {
  const [typeFilter, setTypeFilter] = useState<TypeFilter>('ALL')
  const [statusFilter, setStatusFilter] = useState<'ALL' | AnomalyStatus>('ALL')
  const anomalies = useQuery({ queryKey: queryKeys.anomalies({}), queryFn: () => api.anomalies() })
  const dashboard = useQuery({ queryKey: queryKeys.dashboard, queryFn: api.dashboard })

  if (anomalies.isPending) return <LoadingBlock rows={6} />
  if (anomalies.isError) return <ErrorMessage error={anomalies.error} onRetry={() => anomalies.refetch()} />

  const all = anomalies.data
  const visible = all.filter((anomaly) => (typeFilter === 'ALL' || anomaly.type === typeFilter) && (statusFilter === 'ALL' || anomaly.status === statusFilter))
  const summary = dashboard.data?.anomalies
  const lastAnalysis = dashboard.data?.lastAnalysis

  return (
    <>
      <PageHeader
        title="Anomalías IA"
        description={
          all.length > 0 && summary
            ? `Análisis del ${formatLocalDateTime(lastAnalysis?.finishedAt)} · ${countLabel(summary.cases, 'anomalía detectada', 'anomalías detectadas')} · ${countLabel(summary.highPriority, 'prioritaria', 'prioritarias')} · ${countLabel(summary.dismissed, 'descartada', 'descartadas')}`
            : 'Casos detectados por el último análisis, ordenados por prioridad'
        }
      />
      {all.length === 0 ? (
        <StateMessage
          eyebrow="Sin análisis"
          title="Todavía no hay anomalías"
          description="Ejecuta el análisis para que la IA revise las lecturas y liste los casos ordenados por prioridad."
          action={<RunAnalysisButton />}
        />
      ) : (
        <>
          <div className="flex flex-wrap items-center gap-2.5">
            <SegmentedControl<TypeFilter>
              label="Filtrar por tipo"
              value={typeFilter}
              onChange={setTypeFilter}
              options={typeOptions.map((option) => ({
                ...option,
                count: option.value === 'ALL' ? all.length : all.filter((anomaly) => anomaly.type === option.value).length,
              }))}
            />
            <label className="flex w-full items-center gap-2 text-sm text-muted sm:ml-auto sm:w-auto sm:text-[13px]">
              Estado
              <select
                id="anomaly-status-filter"
                value={statusFilter}
                onChange={(event) => setStatusFilter(event.target.value as 'ALL' | AnomalyStatus)}
                className="h-11 flex-1 rounded-lg border border-line bg-surface px-2 text-sm font-semibold text-ink sm:h-8 sm:flex-none sm:text-[13px]"
              >
                <option value="ALL">Todos</option>
                {(Object.keys(anomalyStatusLabel) as AnomalyStatus[]).map((status) => (
                  <option key={status} value={status}>
                    {anomalyStatusLabel[status]}
                  </option>
                ))}
              </select>
            </label>
          </div>
          {visible.length === 0 ? (
            <StateMessage eyebrow="Sin resultados" title="Ningún caso coincide con los filtros" description="Cambia el tipo o el estado para ver otros casos." />
          ) : (
            <>
              <AnomalyCardList anomalies={visible} />
              <AnomalyTable anomalies={visible} />
            </>
          )}
        </>
      )}
    </>
  )
}
