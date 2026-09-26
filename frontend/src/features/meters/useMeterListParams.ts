import { useSearchParams } from 'react-router'
import type { MeterListParams } from '@/lib/api'
import type { MeterSortField, MeterStatusFilter, SortDirection } from '@/lib/types'

const statusFilters: MeterStatusFilter[] = ['all', 'normal', 'alert', 'critical']
const sortFields: MeterSortField[] = ['severity', 'consumption', 'variation', 'meterId']

function pick<T extends string>(value: string | null, allowed: T[], fallback: T): T {
  return allowed.includes(value as T) ? (value as T) : fallback
}

export function useMeterListParams() {
  const [searchParams, setSearchParams] = useSearchParams()

  const params: Required<MeterListParams> = {
    status: pick(searchParams.get('status'), statusFilters, 'all'),
    search: searchParams.get('search') ?? '',
    sortBy: pick(searchParams.get('sortBy'), sortFields, 'severity'),
    direction: pick<SortDirection>(searchParams.get('direction'), ['descending', 'ascending'], 'descending'),
  }

  function update(changes: Partial<MeterListParams>) {
    const next = { ...params, ...changes }
    setSearchParams(
      Object.fromEntries(Object.entries(next).filter(([, value]) => value !== '' && value !== undefined)) as Record<string, string>,
      { replace: true },
    )
  }

  return { params, update }
}
