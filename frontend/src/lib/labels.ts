import type { AnomalyStatus, AnomalyType, MeterStatus, Severity } from './types'

export type Tone = 'critical' | 'alert' | 'ok' | 'quality' | 'explainable' | 'dismissed' | 'accent'

export const meterStatusLabel: Record<MeterStatus, string> = {
  NOT_ANALYZED: 'Sin analizar',
  OK: 'Normal',
  ALERT: 'Alerta',
  CRITICAL: 'Crítico',
}

export const meterStatusTone: Record<MeterStatus, Tone> = {
  NOT_ANALYZED: 'dismissed',
  OK: 'ok',
  ALERT: 'alert',
  CRITICAL: 'critical',
}

export const anomalyTypeLabel: Record<AnomalyType, string> = {
  REAL_ANOMALY: 'Anomalía real',
  DATA_QUALITY: 'Calidad de datos',
  EXPLAINABLE_ANOMALY: 'Anomalía explicable',
  FALSE_POSITIVE: 'Falso positivo',
}

export const anomalyTypeTone: Record<AnomalyType, Tone> = {
  REAL_ANOMALY: 'critical',
  DATA_QUALITY: 'quality',
  EXPLAINABLE_ANOMALY: 'explainable',
  FALSE_POSITIVE: 'dismissed',
}

export const shortActionLabel: Record<AnomalyType, string> = {
  REAL_ANOMALY: 'Investigar',
  DATA_QUALITY: 'Validar',
  EXPLAINABLE_ANOMALY: 'Validar operación',
  FALSE_POSITIVE: 'No escalar',
}

export const severityLabel: Record<Severity, string> = {
  HIGH: 'Alta',
  MEDIUM: 'Media',
  LOW: 'Baja',
}

const eventTypeNames: Record<string, string> = {
  OPERATIONAL_CHANGE: 'Cambio operativo',
  SCHEDULED_OUTAGE: 'Parada programada',
  DATA_QUALITY: 'Falla de datos',
  UNKNOWN: 'Sin evento operativo',
}

export const eventTypeLabel = (type: string) => eventTypeNames[type] ?? type

const eventTypeColors: Record<string, string> = {
  SCHEDULED_OUTAGE: 'var(--color-dismissed)',
  OPERATIONAL_CHANGE: 'var(--color-explainable)',
  DATA_QUALITY: 'var(--color-quality)',
  UNKNOWN: 'var(--color-muted)',
}

export const eventTypeColor = (type: string) => eventTypeColors[type] ?? 'var(--color-muted)'

export const severityLevel: Record<Severity, number> = {
  HIGH: 3,
  MEDIUM: 2,
  LOW: 1,
}

export const anomalyStatusLabel: Record<AnomalyStatus, string> = {
  OPEN: 'Abierta',
  INVESTIGATING: 'En investigación',
  RESOLVED: 'Resuelta',
  DISMISSED: 'Descartada',
}

export const allowedTransitions: Record<AnomalyStatus, AnomalyStatus[]> = {
  OPEN: ['INVESTIGATING', 'RESOLVED', 'DISMISSED'],
  INVESTIGATING: ['RESOLVED', 'DISMISSED', 'OPEN'],
  RESOLVED: ['OPEN'],
  DISMISSED: ['OPEN'],
}

export const transitionActionLabel: Record<AnomalyStatus, string> = {
  OPEN: 'Reabrir',
  INVESTIGATING: 'Iniciar investigación',
  RESOLVED: 'Marcar resuelta',
  DISMISSED: 'Descartar',
}

export const toneClasses: Record<Tone, { soft: string; text: string; solid: string; border: string }> = {
  critical: { soft: 'bg-critical-soft', text: 'text-critical', solid: 'bg-critical', border: 'border-critical' },
  alert: { soft: 'bg-alert-soft', text: 'text-alert', solid: 'bg-alert', border: 'border-alert' },
  ok: { soft: 'bg-ok-soft', text: 'text-ok', solid: 'bg-ok', border: 'border-ok' },
  quality: { soft: 'bg-quality-soft', text: 'text-quality', solid: 'bg-quality', border: 'border-quality' },
  explainable: { soft: 'bg-explainable-soft', text: 'text-explainable', solid: 'bg-explainable', border: 'border-explainable' },
  dismissed: { soft: 'bg-dismissed-soft', text: 'text-dismissed', solid: 'bg-dismissed', border: 'border-dismissed' },
  accent: { soft: 'bg-accent-soft', text: 'text-accent', solid: 'bg-accent', border: 'border-accent' },
}

export const meterStatusColor: Record<MeterStatus, string> = {
  CRITICAL: 'var(--color-critical)',
  ALERT: 'var(--color-alert)',
  OK: 'var(--color-muted)',
  NOT_ANALYZED: 'var(--color-muted)',
}

export const anomalyStatusTone: Record<AnomalyStatus, Tone> = {
  OPEN: 'alert',
  INVESTIGATING: 'accent',
  RESOLVED: 'ok',
  DISMISSED: 'dismissed',
}
