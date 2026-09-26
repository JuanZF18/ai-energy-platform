import { Link } from 'react-router'
import { Card, CardHeader } from '@/components/ui/Card'
import type { MeterListItem, MeterStatus } from '@/lib/types'

const tileClasses: Record<MeterStatus, string> = {
  CRITICAL: 'border-critical bg-critical-soft text-critical',
  ALERT: 'border-alert bg-alert-soft text-alert',
  OK: 'border-line bg-surface-2 text-ink',
  NOT_ANALYZED: 'border-line bg-surface-2 text-muted',
}

const statusGroups = [
  { status: 'CRITICAL', filter: 'critical', dot: 'bg-critical', label: 'Crítico', meaning: 'anomalía real: atender primero' },
  { status: 'ALERT', filter: 'alert', dot: 'bg-alert', label: 'En alerta', meaning: 'revisar datos u operación' },
  { status: 'OK', filter: 'normal', dot: 'bg-ok', label: 'Normal', meaning: 'consumo dentro de lo esperado' },
] as const

export function FleetStatus({ meters }: { meters: MeterListItem[] }) {
  const byId = [...meters].sort((left, right) => left.meterId.localeCompare(right.meterId))

  return (
    <Card>
      <CardHeader title="Estado de la flota" aside="Toca un medidor para ver su detalle" />
      <div className="grid grid-cols-3 gap-1.5 sm:grid-cols-4">
        {byId.map((meter) => (
          <Link
            key={meter.meterId}
            to={`/meters/${meter.meterId}`}
            title={`${meter.meterId} · ${meter.name}`}
            className={`grid rounded-lg border px-2 py-1.5 hover:brightness-95 ${tileClasses[meter.status]}`}
          >
            <b className="font-mono text-xs">{meter.meterId}</b>
            <span className="truncate text-[10.5px] text-muted">{meter.name}</span>
          </Link>
        ))}
      </div>
      <ul className="grid gap-1 border-t border-line-soft pt-3">
        {statusGroups.map((group) => {
          const count = meters.filter((meter) => meter.status === group.status).length
          return (
            <li key={group.status}>
              <Link
                to={`/meters?status=${group.filter}`}
                className="grid grid-cols-[10px_24px_1fr_auto] items-center gap-2 rounded-md px-1.5 py-1 text-[12.5px] hover:bg-surface-2"
              >
                <span className={`size-2.5 rounded-sm ${group.dot}`} aria-hidden />
                <b className="font-mono tabular">{count}</b>
                <span>
                  <b className="font-semibold">{group.label}</b> <span className="text-muted">· {group.meaning}</span>
                </span>
                <span className="text-accent" aria-hidden>
                  →
                </span>
              </Link>
            </li>
          )
        })}
      </ul>
    </Card>
  )
}
