import { Area, AreaChart, CartesianGrid, ReferenceLine, ResponsiveContainer, Tooltip, XAxis, YAxis } from 'recharts'
import { Card, CardHeader } from '@/components/ui/Card'
import { formatDayLabel, formatKwh, formatNumber, formatPlantDay } from '@/lib/format'
import { eventTypeColor, eventTypeLabel } from '@/lib/labels'
import type { DashboardSummary } from '@/lib/types'

export function FleetConsumptionChart({ summary }: { summary: DashboardSummary }) {
  const data = summary.dailyConsumption.map((day) => ({ label: formatDayLabel(day.day), consumption: day.consumptionKwh }))
  const values = data.map((day) => day.consumption)
  const lowest = Math.floor(Math.min(...values) / 1000) * 1000
  const highest = Math.ceil(Math.max(...values) / 1000) * 1000
  const ticks = Array.from({ length: (highest - lowest) / 1000 + 1 }, (_, index) => lowest + index * 1000)

  return (
    <Card>
      <CardHeader title="Consumo diario de la flota" aside={`kWh por día · la escala empieza en ${formatNumber(lowest)} kWh, no en 0, para mostrar mejor la variación`} />
      <div className="h-[260px] sm:h-[220px]" role="img" aria-label="Consumo diario total de los 12 medidores durante el período">
        <ResponsiveContainer width="100%" height="100%">
          <AreaChart data={data} margin={{ top: 8, right: 12, bottom: 0, left: 0 }}>
            <CartesianGrid stroke="var(--color-line-soft)" vertical={false} />
            <XAxis dataKey="label" tick={{ fontSize: 11, fill: 'var(--color-muted)' }} tickLine={false} axisLine={false} />
            <YAxis
              width={56}
              domain={[lowest, highest]}
              ticks={ticks}
              tickFormatter={(value: number) => formatNumber(value)}
              tick={{ fontSize: 11, fill: 'var(--color-muted)' }}
              tickLine={false}
              axisLine={false}
            />
            <Tooltip formatter={(value) => [formatKwh(Number(value)), 'Consumo']} labelFormatter={(label) => `Día ${label}`} />
            <Area type="monotone" dataKey="consumption" stroke="var(--color-accent)" strokeWidth={2} fill="var(--color-accent)" fillOpacity={0.12} />
            {summary.events.map((event) => (
              <ReferenceLine key={`${event.meterId}-${event.timestamp}`} x={formatPlantDay(event.timestamp)} stroke={eventTypeColor(event.type)} strokeDasharray="4 3" />
            ))}
          </AreaChart>
        </ResponsiveContainer>
      </div>
      <p className="text-xs text-muted">Las líneas punteadas marcan los eventos registrados:</p>
      <ul className="flex flex-wrap gap-x-4 gap-y-1 text-xs text-muted">
        {summary.events.map((event) => (
          <li key={`${event.meterId}-${event.timestamp}`} className="flex items-center gap-1.5">
            <span className="h-3 border-l-2 border-dashed" style={{ borderColor: eventTypeColor(event.type) }} aria-hidden />
            <span className="font-mono font-semibold text-ink">{event.meterId}</span>
            {eventTypeLabel(event.type)} · {formatPlantDay(event.timestamp)}
          </li>
        ))}
      </ul>
    </Card>
  )
}
