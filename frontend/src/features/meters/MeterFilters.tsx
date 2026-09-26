import { ArrowDownWideNarrow, ArrowUpNarrowWide, Search } from 'lucide-react'
import { SegmentedControl } from '@/components/ui/SegmentedControl'
import type { MeterListParams } from '@/lib/api'
import type { DashboardSummary, MeterSortField, MeterStatusFilter } from '@/lib/types'

const sortOptions: { value: MeterSortField; label: string }[] = [
  { value: 'severity', label: 'Severidad' },
  { value: 'consumption', label: 'Consumo' },
  { value: 'variation', label: 'Variación' },
  { value: 'meterId', label: 'Medidor' },
]

interface MeterFiltersProps {
  params: Required<MeterListParams>
  counts: DashboardSummary['meters'] | undefined
  searchText: string
  onSearchTextChange: (value: string) => void
  onChange: (changes: Partial<MeterListParams>) => void
}

export function MeterFilters({ params, counts, searchText, onSearchTextChange, onChange }: MeterFiltersProps) {
  const isDescending = params.direction === 'descending'

  return (
    <div className="flex flex-wrap items-center gap-2.5">
      <SegmentedControl<MeterStatusFilter>
        label="Filtrar por estado"
        value={params.status}
        onChange={(status) => onChange({ status })}
        options={[
          { value: 'all', label: 'Todos', count: counts?.total },
          { value: 'normal', label: 'Normales', count: counts?.ok },
          { value: 'alert', label: 'Alertas', count: counts?.alert },
          { value: 'critical', label: 'Críticas', count: counts?.critical },
        ]}
      />
      <label className="flex min-w-[280px] items-center gap-2 rounded-lg border border-line bg-surface px-2.5">
        <Search className="size-4 text-faint" aria-hidden />
        <input
          id="meter-search"
          value={searchText}
          onChange={(event) => onSearchTextChange(event.target.value)}
          placeholder="Buscar por meter_id, p. ej. M-109"
          aria-label="Buscar por meter_id"
          className="h-8 w-full bg-transparent text-[13px] outline-none placeholder:text-faint"
        />
      </label>
      <div className="ml-auto flex items-center gap-1.5">
        <label htmlFor="meter-sort" className="text-[13px] text-muted">
          Ordenar por
        </label>
        <select
          id="meter-sort"
          value={params.sortBy}
          onChange={(event) => onChange({ sortBy: event.target.value as MeterSortField })}
          className="h-8 rounded-lg border border-line bg-surface px-2 text-[13px] font-semibold"
        >
          {sortOptions.map((option) => (
            <option key={option.value} value={option.value}>
              {option.label}
            </option>
          ))}
        </select>
        <button
          type="button"
          onClick={() => onChange({ direction: isDescending ? 'ascending' : 'descending' })}
          className="grid size-8 place-items-center rounded-lg border border-line bg-surface hover:bg-surface-2"
          aria-label={isDescending ? 'Orden descendente, cambiar a ascendente' : 'Orden ascendente, cambiar a descendente'}
        >
          {isDescending ? <ArrowDownWideNarrow className="size-4" /> : <ArrowUpNarrowWide className="size-4" />}
        </button>
      </div>
    </div>
  )
}
