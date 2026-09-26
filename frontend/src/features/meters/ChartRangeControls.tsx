import { SegmentedControl } from '@/components/ui/SegmentedControl'
import type { ReadingGranularity } from '@/lib/types'

export type ChartRange = '14d' | '7d' | '48h'

export const rangeHours: Record<ChartRange, number> = { '14d': 336, '7d': 168, '48h': 48 }

interface ChartRangeControlsProps {
  granularity: ReadingGranularity
  range: ChartRange
  onGranularityChange: (value: ReadingGranularity) => void
  onRangeChange: (value: ChartRange) => void
}

export function ChartRangeControls({ granularity, range, onGranularityChange, onRangeChange }: ChartRangeControlsProps) {
  return (
    <div className="flex flex-wrap gap-2">
      <SegmentedControl<ReadingGranularity>
        label="Granularidad"
        value={granularity}
        onChange={onGranularityChange}
        options={[
          { value: 'hour', label: 'Hora' },
          { value: 'day', label: 'Día' },
        ]}
      />
      <SegmentedControl<ChartRange>
        label="Rango"
        value={range}
        onChange={onRangeChange}
        options={[
          { value: '14d', label: '14 días' },
          { value: '7d', label: '7 días' },
          { value: '48h', label: '48 h' },
        ]}
      />
    </div>
  )
}
