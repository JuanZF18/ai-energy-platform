import { useState } from 'react'
import type { FormEvent } from 'react'
import { useQuery } from '@tanstack/react-query'
import { Search } from 'lucide-react'
import { useNavigate } from 'react-router'
import { api } from '@/lib/api'
import { queryKeys } from '@/lib/queryKeys'

export function MeterSearch() {
  const navigate = useNavigate()
  const [term, setTerm] = useState('')
  const meters = useQuery({ queryKey: queryKeys.meters({}), queryFn: () => api.meters() })

  function handleSubmit(event: FormEvent) {
    event.preventDefault()
    const normalized = term.trim().toUpperCase()
    if (!normalized) return
    const match = meters.data?.find((meter) => meter.meterId === normalized || meter.meterId.endsWith(normalized))
    navigate(match ? `/meters/${match.meterId}` : `/meters?search=${encodeURIComponent(normalized)}`)
    setTerm('')
  }

  return (
    <form role="search" onSubmit={handleSubmit} className="hidden items-center gap-2 rounded-lg border border-line bg-surface-2 px-2.5 sm:flex">
      <Search className="size-4 text-faint" aria-hidden />
      <input
        id="global-meter-search"
        list="meter-ids"
        value={term}
        onChange={(event) => setTerm(event.target.value)}
        placeholder="Buscar medidor…"
        aria-label="Buscar medidor por meter_id"
        className="h-8 w-40 bg-transparent text-[13px] outline-none placeholder:text-faint"
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
