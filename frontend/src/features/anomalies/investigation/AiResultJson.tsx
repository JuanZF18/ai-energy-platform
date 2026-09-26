import type { AnomalyListItem } from '@/lib/types'

export function AiResultJson({ summary }: { summary: AnomalyListItem }) {
  const result = {
    meter_id: summary.meterId,
    anomaly: summary.anomaly,
    type: summary.type,
    severity: summary.severity,
    confidence: summary.confidence,
    reason: summary.reason,
    recommended_action: summary.recommendedAction,
  }

  return (
    <details className="rounded-xl border border-line bg-surface">
      <summary className="cursor-pointer px-4 py-3 text-[13px] font-semibold">Ver el resultado de la IA en JSON</summary>
      <pre className="overflow-x-auto border-t border-line bg-surface-2 px-4 py-3 font-mono text-xs leading-relaxed">{JSON.stringify(result, null, 2)}</pre>
    </details>
  )
}
