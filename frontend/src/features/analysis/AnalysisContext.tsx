import { createContext, useContext, useState } from 'react'
import type { ReactNode } from 'react'
import { useMutation, useQuery } from '@tanstack/react-query'
import { api } from '@/lib/api'
import { queryKeys } from '@/lib/queryKeys'
import { AnalysisPanel } from './AnalysisPanel'
import { isFinished, useAnalysisRun } from './useAnalysisRun'

interface AnalysisContextValue {
  isRunning: boolean
  isStarting: boolean
  startAnalysis: () => void
  openPanel: () => void
}

const AnalysisContext = createContext<AnalysisContextValue | null>(null)

export function AnalysisProvider({ children }: { children: ReactNode }) {
  const [startedRunId, setStartedRunId] = useState<string | null>(null)
  const [isPanelOpen, setIsPanelOpen] = useState(false)
  const dashboard = useQuery({ queryKey: queryKeys.dashboard, queryFn: api.dashboard })

  const lastAnalysis = dashboard.data?.lastAnalysis
  const runInProgressOnServer = lastAnalysis && (lastAnalysis.status === 'QUEUED' || lastAnalysis.status === 'RUNNING') ? lastAnalysis.id : null
  const runId = startedRunId ?? runInProgressOnServer
  const run = useAnalysisRun(runId)

  const start = useMutation({
    mutationFn: api.startAnalysis,
    onSuccess: (startedRun) => {
      setStartedRunId(startedRun.id)
      setIsPanelOpen(true)
    },
  })

  const value: AnalysisContextValue = {
    isRunning: runId !== null && !isFinished(run.data),
    isStarting: start.isPending,
    startAnalysis: () => start.mutate(),
    openPanel: () => setIsPanelOpen(true),
  }

  return (
    <AnalysisContext.Provider value={value}>
      {children}
      {isPanelOpen && (
        <AnalysisPanel
          run={run.data}
          startError={start.error}
          onRetry={() => start.mutate()}
          onClose={() => setIsPanelOpen(false)}
        />
      )}
    </AnalysisContext.Provider>
  )
}

export function useAnalysis() {
  const context = useContext(AnalysisContext)
  if (!context) throw new Error('useAnalysis debe usarse dentro de AnalysisProvider')
  return context
}
