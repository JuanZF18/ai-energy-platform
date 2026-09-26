import { useDeferredValue, useEffect, useRef, useState } from 'react'
import { keepPreviousData, useQuery } from '@tanstack/react-query'
import { PageHeader } from '@/components/ui/PageHeader'
import { ErrorMessage, LoadingBlock, StateMessage } from '@/components/ui/States'
import { api } from '@/lib/api'
import { queryKeys } from '@/lib/queryKeys'
import { MeterFilters } from './MeterFilters'
import { MeterTable } from './MeterTable'
import { useMeterListParams } from './useMeterListParams'

export default function MetersPage() {
  const { params, update } = useMeterListParams()
  const [searchText, setSearchText] = useState(params.search)
  const deferredSearch = useDeferredValue(searchText)
  const dashboard = useQuery({ queryKey: queryKeys.dashboard, queryFn: api.dashboard })

  const lastSyncedSearch = useRef(params.search)

  useEffect(() => {
    if (deferredSearch === lastSyncedSearch.current) return
    lastSyncedSearch.current = deferredSearch
    update({ search: deferredSearch })
  }, [deferredSearch, update])

  useEffect(() => {
    if (params.search === lastSyncedSearch.current) return
    lastSyncedSearch.current = params.search
    setSearchText(params.search)
  }, [params.search])

  const query = { ...params, search: deferredSearch }
  const meters = useQuery({ queryKey: queryKeys.meters(query), queryFn: () => api.meters(query), placeholderData: keepPreviousData })

  return (
    <>
      <PageHeader title="Medidores" description="Consumo de las últimas 24 horas comparado con el baseline diario de cada medidor" />
      <MeterFilters params={params} counts={dashboard.data?.meters} searchText={searchText} onSearchTextChange={setSearchText} onChange={update} />
      {meters.isPending && <LoadingBlock rows={8} />}
      {meters.isError && <ErrorMessage error={meters.error} onRetry={() => meters.refetch()} />}
      {meters.data?.length === 0 && (
        <StateMessage
          eyebrow="Sin resultados"
          title="Ningún medidor coincide"
          description={`No hay medidores que coincidan con "${searchText}" y el filtro elegido. Prueba con otro identificador o quita los filtros.`}
        />
      )}
      {meters.data && meters.data.length > 0 && <MeterTable meters={meters.data} />}
    </>
  )
}
