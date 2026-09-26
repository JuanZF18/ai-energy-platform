import { formatSignedPercent } from '@/lib/format'

export function VariationValue({ value }: { value: number | null }) {
  const isNotable = value !== null && Math.abs(value) >= 10
  const color = !isNotable ? 'text-muted' : value > 0 ? 'text-critical font-semibold' : 'text-explainable font-semibold'
  return <span className={`font-mono tabular ${color}`}>{formatSignedPercent(value)}</span>
}
