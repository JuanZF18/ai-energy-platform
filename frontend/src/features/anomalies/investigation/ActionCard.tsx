import { useState } from 'react'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { Button } from '@/components/ui/Button'
import { Card, CardHeader } from '@/components/ui/Card'
import { api } from '@/lib/api'
import { allowedTransitions, anomalyStatusLabel, transitionActionLabel } from '@/lib/labels'
import { queryKeys } from '@/lib/queryKeys'
import type { AnomalyDetail, AnomalyStatus } from '@/lib/types'

const statusFlow = (current: AnomalyStatus): AnomalyStatus[] =>
  current === 'DISMISSED' ? ['OPEN', 'DISMISSED'] : ['OPEN', 'INVESTIGATING', 'RESOLVED']

export function ActionCard({ detail }: { detail: AnomalyDetail }) {
  const { summary } = detail
  const queryClient = useQueryClient()
  const [note, setNote] = useState('')

  const changeStatus = useMutation({
    mutationFn: (status: AnomalyStatus) => api.changeAnomalyStatus(summary.id, status, note.trim() || undefined),
    onSuccess: (updated) => {
      queryClient.setQueryData(queryKeys.anomaly(summary.id), updated)
      setNote('')
      ;[['anomalies'], ['dashboard'], ['meters'], ['meter']].forEach((queryKey) => queryClient.invalidateQueries({ queryKey }))
    },
  })

  const transitions = allowedTransitions[summary.status]

  return (
    <Card className="border-accent">
      <CardHeader title="Qué hacer" />
      <p className="text-[15px] font-semibold">{summary.recommendedAction}</p>
      <div className="grid gap-1.5">
        <span className="text-xs font-semibold text-muted">Estado del caso</span>
        <ol className="flex flex-wrap items-center gap-x-1.5 gap-y-1 text-[12px]" aria-label="Estado del caso">
          {statusFlow(summary.status).map((status, index) => {
            const isCurrent = status === summary.status
            return (
              <li key={status} aria-current={isCurrent ? 'step' : undefined} className="flex items-center gap-1.5">
                {index > 0 && <span className="text-faint" aria-hidden>→</span>}
                <span className={`flex items-center gap-1.5 ${isCurrent ? 'font-semibold text-ink' : 'text-faint'}`}>
                  <span className={`size-2 rounded-full ${isCurrent ? 'bg-accent' : 'bg-line'}`} aria-hidden />
                  {anomalyStatusLabel[status]}
                </span>
              </li>
            )
          })}
        </ol>
      </div>
      <label className="grid gap-1 text-xs font-semibold text-muted">
        Nota para el equipo (opcional)
        <textarea
          id="anomaly-note"
          value={note}
          onChange={(event) => setNote(event.target.value)}
          maxLength={500}
          rows={2}
          placeholder="Ej.: mantenimiento revisa la carga en Planta Sur"
          className="rounded-lg border border-line bg-surface-2 px-2.5 py-2 text-[13px] font-normal text-ink"
        />
      </label>
      <div className="grid gap-2">
        {transitions.map((status, index) => (
          <Button key={status} variant={index === 0 ? 'primary' : 'secondary'} disabled={changeStatus.isPending} onClick={() => changeStatus.mutate(status)}>
            {transitionActionLabel[status]}
          </Button>
        ))}
      </div>
      {changeStatus.isError && (
        <p role="alert" className="rounded-lg bg-critical-soft px-3 py-2 text-[13px] text-critical">
          {changeStatus.error.message}
        </p>
      )}
    </Card>
  )
}
