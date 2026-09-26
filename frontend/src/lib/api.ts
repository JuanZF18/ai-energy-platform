import type {
  AnalysisRun,
  AnomalyDetail,
  AnomalyListItem,
  AnomalyStatus,
  AnomalyType,
  DashboardSummary,
  MeterDetail,
  MeterListItem,
  MeterSortField,
  MeterStatusFilter,
  ReadingGranularity,
  ReadingPoint,
  SortDirection,
} from './types'

export class ApiError extends Error {
  readonly status: number

  constructor(status: number, message: string) {
    super(message)
    this.status = status
  }
}

async function request<T>(path: string, init?: RequestInit): Promise<T> {
  const response = await fetch(path, {
    ...init,
    headers: { 'Content-Type': 'application/json', ...init?.headers },
  })

  if (!response.ok) {
    const problem = await response.json().catch(() => null)
    throw new ApiError(response.status, problem?.detail ?? problem?.title ?? 'No pudimos completar la solicitud.')
  }

  return response.json() as Promise<T>
}

function withQuery(path: string, params: Record<string, string | number | undefined>) {
  const query = new URLSearchParams()
  Object.entries(params).forEach(([key, value]) => {
    if (value !== undefined && value !== '') query.set(key, String(value))
  })
  const text = query.toString()
  return text ? `${path}?${text}` : path
}

export interface MeterListParams {
  status?: MeterStatusFilter
  search?: string
  sortBy?: MeterSortField
  direction?: SortDirection
}

export interface ReadingParams {
  from?: string
  to?: string
  granularity?: ReadingGranularity
}

export interface AnomalyListParams {
  type?: AnomalyType
  status?: AnomalyStatus
  limit?: number
}

export const api = {
  dashboard: () => request<DashboardSummary>('/api/dashboard/summary'),
  meters: (params: MeterListParams = {}) => request<MeterListItem[]>(withQuery('/api/meters', { ...params })),
  meter: (meterId: string) => request<MeterDetail>(`/api/meters/${encodeURIComponent(meterId)}`),
  readings: (meterId: string, params: ReadingParams = {}) =>
    request<ReadingPoint[]>(withQuery(`/api/meters/${encodeURIComponent(meterId)}/readings`, { ...params })),
  anomalies: (params: AnomalyListParams = {}) => request<AnomalyListItem[]>(withQuery('/api/anomalies', { ...params })),
  anomaly: (id: string) => request<AnomalyDetail>(`/api/anomalies/${id}`),
  changeAnomalyStatus: (id: string, status: AnomalyStatus, note?: string) =>
    request<AnomalyDetail>(`/api/anomalies/${id}`, { method: 'PATCH', body: JSON.stringify({ status, note }) }),
  startAnalysis: () => request<AnalysisRun>('/api/ai/analyze', { method: 'POST' }),
  analysis: (id: string) => request<AnalysisRun>(`/api/ai/analysis/${id}`),
}
