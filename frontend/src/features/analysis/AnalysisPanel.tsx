import { useEffect } from 'react'
import { useQuery } from '@tanstack/react-query'
import { X } from 'lucide-react'
import { useNavigate } from 'react-router'
import { AiTag, StatusPill } from '@/components/ui/Badges'
import { Button } from '@/components/ui/Button'
import { api } from '@/lib/api'
import { countLabel } from '@/lib/format'
import { queryKeys } from '@/lib/queryKeys'
import type { AnalysisRun } from '@/lib/types'
import { StageList } from './StageList'

interface AnalysisPanelProps {
  run: AnalysisRun | undefined
  startError: Error | null
  onRetry: () => void
  onClose: () => void
}

export function AnalysisPanel({ run, startError, onRetry, onClose }: AnalysisPanelProps) {
  const navigate = useNavigate()
  const isCompleted = run?.status === 'COMPLETED'
  const topAnomaly = useQuery({
    queryKey: queryKeys.anomalies({ limit: 1 }),
    queryFn: () => api.anomalies({ limit: 1 }),
    enabled: isCompleted,
  })

  useEffect(() => {
    const closeOnEscape = (event: KeyboardEvent) => event.key === 'Escape' && onClose()
    window.addEventListener('keydown', closeOnEscape)
    return () => window.removeEventListener('keydown', closeOnEscape)
  }, [onClose])

  function goTo(path: string) {
    onClose()
    navigate(path)
  }

  const first = topAnomaly.data?.[0]

  return (
    <div className="fixed inset-0 z-50 grid place-items-center bg-nav/40 p-4" onClick={onClose}>
      <div
        role="dialog"
        aria-modal="true"
        aria-labelledby="analysis-title"
        className="grid w-full max-w-[560px] gap-4 rounded-2xl border border-line bg-surface p-5 shadow-2xl"
        onClick={(event) => event.stopPropagation()}
      >
        <div className="flex items-center gap-2">
          <AiTag />
          <h2 id="analysis-title" className="text-lg font-bold">Análisis de anomalías</h2>
          <span className="ml-auto">{run && <RunStatusPill run={run} />}</span>
          <button type="button" onClick={onClose} className="rounded-md p-1 text-muted hover:bg-surface-2" aria-label="Cerrar">
            <X className="size-4" />
          </button>
        </div>

        {startError && !run && <p className="rounded-lg bg-critical-soft px-3 py-2 text-[13px] text-critical">{startError.message}</p>}
        {run ? <StageList stages={run.stages} /> : !startError && <p className="text-muted">Preparando el análisis…</p>}

        {isCompleted && run.summary && (
          <div className="grid gap-2 rounded-xl border border-line bg-surface-2 p-4">
            <p className="font-display text-xl font-bold">{run.headline}</p>
            <p className="text-[13px] text-muted">
              {countLabel(run.summary.realAnomalies, 'anomalía real', 'anomalías reales')} ·{' '}
              {countLabel(run.summary.dataQualityIssues, 'problema de calidad de datos', 'problemas de calidad de datos')} ·{' '}
              {countLabel(run.summary.explainableAnomalies, 'anomalía explicable', 'anomalías explicables')} ·{' '}
              {countLabel(run.summary.falsePositives, 'falso positivo descartado', 'falsos positivos descartados')}
            </p>
            <div className="flex flex-wrap gap-2 pt-1">
              {first && <Button onClick={() => goTo(`/anomalies/${first.id}`)}>Ver {first.meterId} (prioridad 1) →</Button>}
              <Button variant="secondary" onClick={() => goTo('/anomalies')}>
                Ver todas las anomalías
              </Button>
            </div>
          </div>
        )}

        {run?.status === 'FAILED' && (
          <div className="grid gap-2 rounded-xl bg-critical-soft p-4 text-[13px] text-critical">
            <p>{run.error}</p>
            <Button variant="secondary" className="justify-self-start" onClick={onRetry}>
              Reintentar análisis
            </Button>
          </div>
        )}

        <p className="text-xs text-faint">Puedes cerrar este panel: el análisis sigue y su estado queda en la barra superior.</p>
      </div>
    </div>
  )
}

function RunStatusPill({ run }: { run: AnalysisRun }) {
  if (run.status === 'COMPLETED') {
    const seconds = run.startedAt && run.finishedAt ? (Date.parse(run.finishedAt) - Date.parse(run.startedAt)) / 1000 : null
    return <StatusPill tone="ok" label={seconds ? `Completado · ${seconds.toFixed(1).replace('.', ',')} s` : 'Completado'} />
  }
  if (run.status === 'FAILED') return <StatusPill tone="critical" label="Falló" />
  return <StatusPill tone="accent" label="En curso" />
}
