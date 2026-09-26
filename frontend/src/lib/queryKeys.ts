import type { AnomalyListParams, MeterListParams, ReadingParams } from './api'

export const queryKeys = {
  dashboard: ['dashboard'] as const,
  meters: (params: MeterListParams) => ['meters', params] as const,
  meter: (meterId: string) => ['meter', meterId] as const,
  readings: (meterId: string, params: ReadingParams) => ['readings', meterId, params] as const,
  anomalies: (params: AnomalyListParams) => ['anomalies', params] as const,
  anomaly: (id: string) => ['anomaly', id] as const,
  analysis: (id: string) => ['analysis', id] as const,
}

export const analysisDependentKeys = [['dashboard'], ['meters'], ['meter'], ['readings'], ['anomalies'], ['anomaly']]
