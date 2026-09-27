export type MeterStatus = 'NOT_ANALYZED' | 'OK' | 'ALERT' | 'CRITICAL'
export type AnomalyType = 'REAL_ANOMALY' | 'DATA_QUALITY' | 'EXPLAINABLE_ANOMALY' | 'FALSE_POSITIVE'
export type Severity = 'LOW' | 'MEDIUM' | 'HIGH'
export type AnomalyStatus = 'OPEN' | 'INVESTIGATING' | 'RESOLVED' | 'DISMISSED'
export type AnalysisRunStatus = 'QUEUED' | 'RUNNING' | 'COMPLETED' | 'FAILED'
export type AnalysisStage = 'READINGS' | 'BASELINE' | 'DETECTION' | 'CORRELATION' | 'EVENTS' | 'EXPLANATION' | 'RECOMMENDATION'
export type StageState = 'PENDING' | 'RUNNING' | 'DONE'
export type ExplanationSource = 'TEMPLATE' | 'LANGUAGE_MODEL'

export type MeterStatusFilter = 'all' | 'normal' | 'alert' | 'critical'
export type MeterSortField = 'severity' | 'consumption' | 'variation' | 'meterId'
export type SortDirection = 'descending' | 'ascending'
export type ReadingGranularity = 'hour' | 'day'

export interface AnomalyBadge {
  id: string
  type: AnomalyType
  severity: Severity
  status: AnomalyStatus
  priority: number
}

export interface MeterListItem {
  meterId: string
  name: string
  location: string
  status: MeterStatus
  currentDailyKwh: number
  baselineDailyKwh: number | null
  variationPercent: number | null
  anomaly: AnomalyBadge | null
  dailyConsumption: number[]
}

export interface MeterEvent {
  meterId: string
  timestamp: string
  type: string
  description: string
}

export interface MeterDetail {
  meterId: string
  name: string
  location: string
  status: MeterStatus
  currentDailyKwh: number
  baselineDailyKwh: number | null
  variationPercent: number | null
  hourlyBaseline: number[]
  readingsCount: number
  firstReadingAt: string | null
  lastReadingAt: string | null
  anomalies: AnomalyListItem[]
  events: MeterEvent[]
}

export interface ReadingPoint {
  timestamp: string
  consumptionKwh: number
  voltageV: number
  currentA: number
  powerFactor: number
  expectedKwh: number | null
  isSuspect: boolean
}

export interface AnomalyListItem {
  id: string
  rank: number
  meterId: string
  meterName: string
  anomaly: boolean
  type: AnomalyType
  severity: Severity
  confidence: number
  confidenceLabel: string
  priority: number
  reason: string
  recommendedAction: string
  status: AnomalyStatus
  detectedAt: string
}

export interface EvidenceWindow {
  start: string
  end: string
  hours: number
  isOngoing: boolean
}

export interface ConsumptionEvidence {
  baselineDailyKwh: number
  observedDailyKwh: number
  variationPercent: number
  meanHourlyDeviationPercent: number
}

export interface VariableChange {
  variable: string
  unit: string
  before: number
  after: number
  changePercent: number
}

export interface EventEvidence {
  type: string
  timestamp: string
  description: string
  explainsChange: boolean
}

export interface DataQualityEvidence {
  suspectReadings: number
  minimumVoltage: number
  maximumVoltage: number
  repeatedPowerFactors: number[]
  repeatsEveryHours: number | null
}

export interface ConfidenceBreakdown {
  signal: number
  persistence: number
  corroboration: number
  score: number
}

export interface Evidence {
  window: EvidenceWindow
  consumption: ConsumptionEvidence
  changedVariables: VariableChange[]
  events: EventEvidence[]
  dataQuality: DataQualityEvidence | null
  confidence: ConfidenceBreakdown
  facts: string[]
}

export interface Explanation {
  summary: string
  whatChanged: string[]
  possibleCauses: string[]
  nextSteps: string[]
  source: ExplanationSource
}

export interface StatusChange {
  from: AnomalyStatus
  to: AnomalyStatus
  note: string | null
  changedBy: string
  changedAt: string
}

export interface AnomalyDetail {
  summary: AnomalyListItem
  meterLocation: string
  evidence: Evidence
  explanation: Explanation | null
  history: StatusChange[]
}

export interface AnalysisSummary {
  metersAnalyzed: number
  readingsAnalyzed: number
  cases: number
  anomalies: number
  priorityCases: number
  realAnomalies: number
  dataQualityIssues: number
  explainableAnomalies: number
  falsePositives: number
}

export interface StageProgress {
  stage: AnalysisStage
  label: string
  state: StageState
  durationSeconds: number | null
}

export interface AnalysisRun {
  id: string
  status: AnalysisRunStatus
  currentStage: AnalysisStage | null
  stages: StageProgress[]
  requestedAt: string
  startedAt: string | null
  finishedAt: string | null
  summary: AnalysisSummary | null
  headline: string | null
  error: string | null
}

export interface DashboardSummary {
  meters: { total: number; ok: number; alert: number; critical: number; notAnalyzed: number }
  consumption: { periodKwh: number; lastDayKwh: number; periodStart: string | null; periodEnd: string | null }
  anomalies: { cases: number; anomalies: number; dismissed: number; highPriority: number }
  aggregateConfidence: number | null
  lastAnalysis: {
    id: string
    status: AnalysisRunStatus
    requestedAt: string
    finishedAt: string | null
    durationSeconds: number | null
  } | null
  dailyConsumption: { day: string; consumptionKwh: number }[]
  events: MeterEvent[]
}

export type AuthMode = 'FIREBASE' | 'DEMO'

export interface FirebaseWebConfig {
  apiKey: string
  authDomain: string
  projectId: string
  appId: string
}

export interface ClientConfig {
  authMode: AuthMode
  firebase: FirebaseWebConfig | null
  demoAccount: { email: string; password: string }
}

export interface DemoLoginResponse {
  token: string
  name: string
  email: string
}
