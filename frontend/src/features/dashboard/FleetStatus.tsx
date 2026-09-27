import { Link } from 'react-router'
import { ButtonLink } from '@/components/ui/Button'
import { Card, CardHeader } from '@/components/ui/Card'
import type { MeterListItem, MeterStatus } from '@/lib/types'

const maximumTiles = 12

const tileClasses: Record<MeterStatus, string> = {
  CRITICAL: 'border-critical bg-critical-soft text-critical',
  ALERT: 'border-alert bg-alert-soft text-alert',
  OK: 'border-line bg-surface-2 text-ink',
  NOT_ANALYZED: 'border-line bg-surface-2 text-muted',
}

const attentionOrder: Record<MeterStatus, number> = { CRITICAL: 0, ALERT: 1, NOT_ANALYZED: 2, OK: 3 }

const statusGroups = [
  { status: 'CRITICAL', filter: 'critical', dot: 'bg-critical', label: 'Crítico', meaning: 'anomalía real: atender primero', list: 'críticos' },
  { status: 'ALERT', filter: 'alert', dot: 'bg-alert', label: 'En alerta', meaning: 'revisar datos u operación', list: 'en alerta' },
  { status: 'OK', filter: 'normal', dot: 'bg-ok', label: 'Normal', meaning: 'consumo dentro de lo esperado', list: 'normales' },
] as const

export function FleetStatus({ meters }: { meters: MeterListItem[] }) {
  const byAttention = [...meters].sort(
    (left, right) => attentionOrder[left.status] - attentionOrder[right.status] || left.meterId.localeCompare(right.meterId),
  )
  const tiles = byAttention.slice(0, maximumTiles)
  const hiddenCount = meters.length - tiles.length

  return (
    <Card>
      <CardHeader title="Estado de la flota" aside="Primero los que requieren atención. Toca uno para ver su detalle" />
      <div className="grid grid-cols-2 gap-1.5 min-[420px]:grid-cols-3 sm:grid-cols-4">
        {tiles.map((meter) => (
          <Link
            key={meter.meterId}
            to={`/meters/${meter.meterId}`}
            title={`${meter.meterId} · ${meter.name}`}
            className={`grid content-start gap-0.5 rounded-lg border px-2 py-1.5 hover:brightness-95 ${tileClasses[meter.status]}`}
          >
            <b className="font-mono text-xs">{meter.meterId}</b>
            <span className="line-clamp-2 text-xs leading-tight break-words hyphens-auto text-muted">{meter.name}</span>
          </Link>
        ))}
      </div>
      {hiddenCount > 0 && (
        <ButtonLink to="/meters" variant="secondary" size="sm">
          Ver los {meters.length} medidores ({hiddenCount} más)
        </ButtonLink>
      )}
      <ul className="grid gap-1.5 border-t border-line-soft pt-3">
        {statusGroups.map((group) => {
          const count = meters.filter((meter) => meter.status === group.status).length
          return (
            <li key={group.status} className="grid grid-cols-[10px_24px_1fr_auto] items-center gap-2 text-[13px]">
              <span className={`size-2.5 rounded-sm ${group.dot}`} aria-hidden />
              <b className="font-mono tabular">{count}</b>
              <span>
                <b className="font-semibold">{group.label}</b> <span className="text-muted">· {group.meaning}</span>
              </span>
              <ButtonLink to={`/meters?status=${group.filter}`} variant="ghost" className="border border-line">
                <span aria-hidden>Ver</span>
                <span className="sr-only">Ver medidores {group.list}</span>
              </ButtonLink>
            </li>
          )
        })}
      </ul>
    </Card>
  )
}
