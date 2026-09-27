import { useQuery } from '@tanstack/react-query'
import { Link, useParams } from 'react-router'
import { PageHeader } from '@/components/ui/PageHeader'
import { ErrorMessage, LoadingBlock, StateMessage } from '@/components/ui/States'
import { ApiError, api } from '@/lib/api'
import { formatLocalDateTime } from '@/lib/format'
import { anomalyTypeLabel, severityLabel } from '@/lib/labels'
import { queryKeys } from '@/lib/queryKeys'
import { ActionCard } from './investigation/ActionCard'
import { AiResultJson } from './investigation/AiResultJson'
import { BaselineComparison } from './investigation/BaselineComparison'
import { CaseTimeline } from './investigation/CaseTimeline'
import { ChangedVariablesTable } from './investigation/ChangedVariablesTable'
import { DecisionCard } from './investigation/DecisionCard'
import { EvidenceCards } from './investigation/EvidenceCards'
import { ExplanationCard } from './investigation/ExplanationCard'
import { FindingCard } from './investigation/FindingCard'

export default function InvestigationPage() {
  const { anomalyId = '' } = useParams()
  const investigation = useQuery({ queryKey: queryKeys.anomaly(anomalyId), queryFn: () => api.anomaly(anomalyId), retry: false })

  if (investigation.isPending) return <LoadingBlock rows={10} />
  if (investigation.error instanceof ApiError && investigation.error.status === 404) return <AnomalyNotFound />
  if (investigation.isError) return <ErrorMessage error={investigation.error} onRetry={() => investigation.refetch()} />

  const detail = investigation.data
  const { summary } = detail

  return (
    <>
      <PageHeader
        title={
          <span>
            <span className="font-mono">{summary.meterId}</span> · {anomalyTypeLabel[summary.type]} · severidad {severityLabel[summary.severity].toLowerCase()}
          </span>
        }
        description={`${summary.meterName} · ${detail.meterLocation} · detectada el ${formatLocalDateTime(summary.detectedAt)}`}
        actions={
          <Link
            to={`/meters/${summary.meterId}`}
            className="inline-flex h-9 items-center rounded-lg border border-line bg-surface px-3.5 text-[13px] font-semibold hover:bg-surface-2"
          >
            Ver medidor
          </Link>
        }
      />
      <div className="grid items-start gap-4 xl:grid-cols-[1fr_330px]">
        <div className="grid min-w-0 gap-3">
          <FindingCard detail={detail} />
        </div>
        <aside className="grid gap-3 xl:sticky xl:top-[72px] xl:col-start-2 xl:row-span-2 xl:row-start-1">
          <ActionCard detail={detail} />
          <DecisionCard detail={detail} />
          <CaseTimeline detail={detail} />
        </aside>
        <div className="grid min-w-0 gap-3 xl:col-start-1 xl:row-start-2">
          <BaselineComparison detail={detail} />
          {detail.explanation && <ExplanationCard explanation={detail.explanation} />}
          <EvidenceCards evidence={detail.evidence} />
          <ChangedVariablesTable evidence={detail.evidence} />
          <AiResultJson summary={summary} />
        </div>
      </div>
    </>
  )
}

function AnomalyNotFound() {
  return (
    <StateMessage
      eyebrow="No encontrada"
      title="Esta anomalía no existe"
      description="Puede que el enlace sea antiguo. Revisa la lista de anomalías del último análisis."
      action={
        <Link to="/anomalies" className="inline-flex h-9 items-center rounded-lg border border-line px-3.5 text-[13px] font-semibold hover:bg-surface-2">
          Ir a Anomalías
        </Link>
      }
    />
  )
}
