import { Link } from 'react-router'
import { Card, CardHeader } from '@/components/ui/Card'
import { meterStatusLabel } from '@/lib/labels'
import type { MeterListItem, MeterStatus } from '@/lib/types'

const tileClasses: Record<MeterStatus, string> = {
  CRITICAL: 'border-critical bg-critical-soft text-critical',
  ALERT: 'border-alert bg-alert-soft text-alert',
  OK: 'border-line bg-surface-2 text-ink',
  NOT_ANALYZED: 'border-line bg-surface-2 text-muted',
}

export function FleetStatus({ meters }: { meters: MeterListItem[] }) {
  const byId = [...meters].sort((left, right) => left.meterId.localeCompare(right.meterId))

  return (
    <Card>
      <CardHeader title="Estado de la flota" aside="Clic para ver el detalle" />
      <div className="grid grid-cols-3 gap-1.5 sm:grid-cols-4">
        {byId.map((meter) => (
          <Link
            key={meter.meterId}
            to={`/meters/${meter.meterId}`}
            title={`${meter.meterId} · ${meter.name}`}
            className={`grid rounded-lg border px-2 py-1.5 hover:brightness-95 ${tileClasses[meter.status]}`}
          >
            <b className="font-mono text-xs">{meter.meterId}</b>
            <span className="text-[10.5px] text-muted">{meterStatusLabel[meter.status]}</span>
          </Link>
        ))}
      </div>
      <div className="flex flex-wrap gap-3 text-[11.5px] text-muted">
        <Legend color="bg-critical" label="Critical" />
        <Legend color="bg-alert" label="Alert" />
        <Legend color="bg-line" label="OK" />
      </div>
    </Card>
  )
}

function Legend({ color, label }: { color: string; label: string }) {
  return (
    <span className="flex items-center gap-1.5">
      <span className={`size-2.5 rounded-sm ${color}`} aria-hidden />
      {label}
    </span>
  )
}
