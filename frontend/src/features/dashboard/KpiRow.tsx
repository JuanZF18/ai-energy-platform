import { StatusPill } from '@/components/ui/Badges'
import { KpiCard } from '@/components/ui/KpiCard'
import { countLabel, formatConfidence, formatKwh, formatLocalDateTime, formatNumber, formatSeconds } from '@/lib/format'
import type { DashboardSummary } from '@/lib/types'

const confidenceLabel = (value: number) => (value >= 0.85 ? 'Alta' : value >= 0.6 ? 'Media' : 'Baja')

interface KpiRowProps {
  summary: DashboardSummary
  hasAnalysis: boolean
  priorityMeterIds: string[] | undefined
}

export function KpiRow({ summary, hasAnalysis, priorityMeterIds }: KpiRowProps) {
  const { meters, consumption, anomalies, aggregateConfidence, lastAnalysis } = summary

  return (
    <div className="grid grid-cols-2 gap-2.5 md:grid-cols-3 xl:grid-cols-6">
      <KpiCard label="Medidores" value={meters.total} hint={hasAnalysis ? `${countLabel(meters.ok, 'normal', 'normales')} · ${meters.alert} en alerta · ${countLabel(meters.critical, 'crítico', 'críticos')}` : 'Sin analizar'}>
        <div className="mt-1 flex h-1.5 gap-0.5 overflow-hidden rounded" aria-hidden>
          <span className="bg-ok" style={{ flex: meters.ok }} />
          <span className="bg-alert" style={{ flex: meters.alert }} />
          <span className="bg-critical" style={{ flex: meters.critical }} />
          <span className="bg-line" style={{ flex: meters.notAnalyzed }} />
        </div>
      </KpiCard>
      <KpiCard
        label="Consumo del período"
        value={formatNumber(consumption.periodKwh)}
        unit="kWh"
        hint={`14 días · último día: ${formatKwh(consumption.lastDayKwh)}`}
      />
      <KpiCard
        label="Casos detectados por la IA"
        value={hasAnalysis ? anomalies.cases : '—'}
        unit={hasAnalysis ? 'casos' : undefined}
        hint={hasAnalysis ? `${anomalies.anomalies} por atender · ${countLabel(anomalies.dismissed, 'falso positivo descartado', 'falsos positivos descartados')}` : 'Ejecuta el análisis'}
      />
      <KpiCard
        label="Atender primero"
        value={hasAnalysis ? <span className="text-critical">{anomalies.highPriority}</span> : '—'}
        highlighted={hasAnalysis && anomalies.highPriority > 0}
        hint={hasAnalysis && priorityMeterIds ? priorityMeterIds.join(' · ') || 'Ninguna' : undefined}
      />
      <KpiCard
        label="Confianza de la IA"
        value={formatConfidence(aggregateConfidence)}
        hint={aggregateConfidence !== null ? `${confidenceLabel(aggregateConfidence)} · qué tan segura está la IA, en promedio` : 'Sin datos'}
      />
      <KpiCard
        label="Último análisis"
        value={<span className="text-lg">{lastAnalysis ? formatLocalDateTime(lastAnalysis.finishedAt ?? lastAnalysis.requestedAt) : '—'}</span>}
        hint={lastAnalysis ? <LastAnalysisStatus status={lastAnalysis.status} seconds={lastAnalysis.durationSeconds} /> : 'Todavía no se ejecuta'}
      />
    </div>
  )
}

function LastAnalysisStatus({ status, seconds }: { status: string; seconds: number | null }) {
  if (status === 'COMPLETED') {
    return (
      <span className="flex items-center gap-1.5">
        <StatusPill tone="ok" label="Completado" /> en {formatSeconds(seconds)}
      </span>
    )
  }
  if (status === 'FAILED') return <StatusPill tone="critical" label="Falló" />
  return <StatusPill tone="accent" label="En curso" />
}
