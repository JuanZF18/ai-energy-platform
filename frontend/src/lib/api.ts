import type {
  AnalysisRun,
  AnomalyDetail,
  AnomalyListItem,
  AnomalyStatus,
  AnomalyType,
  ClientConfig,
  DashboardSummary,
  DemoLoginResponse,
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

const apiBaseUrl = (import.meta.env.VITE_API_URL ?? '').replace(/\/$/, '')

let tokenProvider: () => Promise<string | null> = async () => null
let handleUnauthorized: () => void = () => {}

export function configureApiAuth(provider: () => Promise<string | null>, onUnauthorized: () => void) {
  tokenProvider = provider
  handleUnauthorized = onUnauthorized
}

async function request<T>(path: string, init?: RequestInit): Promise<T> {
  const token = await tokenProvider()
  const response = await fetch(`${apiBaseUrl}${path}`, {
    ...init,
    headers: { 'Content-Type': 'application/json', ...(token ? { Authorization: `Bearer ${token}` } : {}), ...init?.headers },
  })

  if (response.status === 401) {
    handleUnauthorized()
    throw new ApiError(401, 'Tu sesión terminó. Vuelve a iniciar sesión.')
  }

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
  config: () => request<ClientConfig>('/api/config'),
  demoLogin: (email: string, password: string) =>
    request<DemoLoginResponse>('/api/auth/login', { method: 'POST', body: JSON.stringify({ email, password }) }),
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
