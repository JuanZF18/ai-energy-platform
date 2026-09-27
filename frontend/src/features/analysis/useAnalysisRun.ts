import { useEffect, useRef } from 'react'
import { useQuery, useQueryClient } from '@tanstack/react-query'
import { api } from '@/lib/api'
import { analysisDependentKeys, queryKeys } from '@/lib/queryKeys'
import type { AnalysisRun } from '@/lib/types'

const pollingIntervalMs = 250

export function isFinished(run: AnalysisRun | undefined) {
  return run?.status === 'COMPLETED' || run?.status === 'FAILED'
}

export function useAnalysisRun(runId: string | null) {
  const queryClient = useQueryClient()
  const previousStatus = useRef<string | undefined>(undefined)

  const query = useQuery({
    queryKey: queryKeys.analysis(runId ?? 'none'),
    queryFn: () => api.analysis(runId!),
    enabled: runId !== null,
    refetchInterval: (current) => (isFinished(current.state.data) ? false : pollingIntervalMs),
    refetchIntervalInBackground: true,
  })

  const status = query.data?.status
  useEffect(() => {
    if (status === 'COMPLETED' && previousStatus.current !== 'COMPLETED') {
      analysisDependentKeys.forEach((queryKey) => queryClient.invalidateQueries({ queryKey }))
    }
    previousStatus.current = status
  }, [status, queryClient])

  return query
}
