import type { Evidence } from '@/lib/types'

export function confirmsIssue(evidence: Evidence, eventType: string) {
  return evidence.dataQuality !== null && eventType === 'DATA_QUALITY'
}
