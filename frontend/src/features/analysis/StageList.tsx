import { Check } from 'lucide-react'
import { formatSeconds } from '@/lib/format'
import type { AnalysisStage, StageProgress } from '@/lib/types'

const stageDescriptions: Record<AnalysisStage, string> = {
  READINGS: 'Carga las lecturas horarias de los 12 medidores',
  BASELINE: 'Calcula el consumo esperado para cada hora del día',
  DETECTION: 'Busca cambios de consumo y lecturas eléctricas inconsistentes',
  CORRELATION: 'Compara voltaje, corriente y factor de potencia antes y durante cada cambio',
  EVENTS: 'Cruza cada caso con los eventos operativos registrados',
  EXPLANATION: 'Clasifica cada caso y redacta la explicación con su evidencia',
  RECOMMENDATION: 'Asigna la acción recomendada y ordena por prioridad',
}

export function StageList({ stages }: { stages: StageProgress[] }) {
  return (
    <ol className="grid">
      {stages.map((stage, index) => (
        <li key={stage.stage} className="relative grid grid-cols-[26px_1fr_auto] items-start gap-3 py-2">
          {index < stages.length - 1 && <span className="absolute top-8 bottom-[-8px] left-[11px] w-px bg-line" aria-hidden />}
          <StageMarker state={stage.state} position={index + 1} />
          <div>
            <p className="text-[13px] font-semibold">{stage.label}</p>
            <p className="text-xs text-muted">{stageDescriptions[stage.stage]}</p>
          </div>
          <span className="font-mono text-[11px] text-faint">{stage.state === 'DONE' ? formatSeconds(stage.durationSeconds) : ''}</span>
        </li>
      ))}
    </ol>
  )
}

function StageMarker({ state, position }: { state: StageProgress['state']; position: number }) {
  if (state === 'DONE') {
    return (
      <span className="z-10 grid size-[22px] place-items-center rounded-full bg-ok text-white">
        <Check className="size-3" strokeWidth={3} aria-label="Completada" />
      </span>
    )
  }
  if (state === 'RUNNING') {
    return <span className="z-10 grid size-[22px] animate-pulse place-items-center rounded-full bg-accent text-[11px] font-bold text-white ring-4 ring-accent-soft">{position}</span>
  }
  return <span className="z-10 grid size-[22px] place-items-center rounded-full border border-line bg-surface-2 text-[11px] font-bold text-faint">{position}</span>
}
