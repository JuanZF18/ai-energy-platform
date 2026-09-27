import { useState } from 'react'
import type { FormEvent } from 'react'
import { useQuery } from '@tanstack/react-query'
import { Search } from 'lucide-react'
import { useNavigate } from 'react-router'
import { defaultMeterListParams } from '@/features/meters/useMeterListParams'
import { api } from '@/lib/api'
import { queryKeys } from '@/lib/queryKeys'

export function MeterSearch() {
  const navigate = useNavigate()
  const [term, setTerm] = useState('')
  const meters = useQuery({ queryKey: queryKeys.meters(defaultMeterListParams), queryFn: () => api.meters(defaultMeterListParams) })

  function handleSubmit(event: FormEvent) {
    event.preventDefault()
    const normalized = term.trim().toUpperCase()
    if (!normalized) return
    const match = meters.data?.find((meter) => meter.meterId === normalized || meter.meterId.endsWith(normalized))
    navigate(match ? `/meters/${match.meterId}` : `/meters?search=${encodeURIComponent(normalized)}`)
    setTerm('')
  }

  return (
    <form role="search" onSubmit={handleSubmit} className="flex min-w-0 flex-1 items-center gap-2 rounded-lg border border-line bg-surface-2 px-3 focus-within:border-accent md:ml-auto md:max-w-72">
      <Search className="size-5 shrink-0 text-muted md:size-4" aria-hidden />
      <input
        id="global-meter-search"
        list="meter-ids"
        value={term}
        onChange={(event) => setTerm(event.target.value)}
        placeholder="Buscar medidor…"
        aria-label="Buscar medidor por meter_id"
        className="h-11 w-full min-w-0 bg-transparent text-base outline-none placeholder:text-muted md:h-9 md:text-[13px]"
      />
      <datalist id="meter-ids">
        {meters.data?.map((meter) => (
          <option key={meter.meterId} value={meter.meterId}>
            {meter.name}
          </option>
        ))}
      </datalist>
    </form>
  )
}
