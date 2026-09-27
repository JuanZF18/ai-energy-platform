import { Area, CartesianGrid, ComposedChart, Line, ReferenceArea, ReferenceLine, ResponsiveContainer, Scatter, Tooltip, XAxis, YAxis } from 'recharts'
import { formatNumber, formatPlantDateTime } from '@/lib/format'
import { eventTypeColor, eventTypeLabel } from '@/lib/labels'
import type { EvidenceWindow, MeterEvent } from '@/lib/types'
import type { ChartPoint } from './chartData'
import { dayTicks, formatTick } from './chartData'

interface ConsumptionChartProps {
  points: ChartPoint[]
  hoursPerTick: number
  anomalyWindow?: EvidenceWindow
  events: MeterEvent[]
  height?: number
}

export function ConsumptionChart({ points, hoursPerTick, anomalyWindow, events, height = 240 }: ConsumptionChartProps) {
  if (points.length === 0) return <p className="text-[13px] text-muted">No hay lecturas en este rango.</p>

  return (
    <div style={{ height }} role="img" aria-label="Consumo medido frente al consumo esperado">
      <ResponsiveContainer width="100%" height="100%">
        <ComposedChart data={points} margin={{ top: 18, right: 12, bottom: 0, left: 0 }}>
          <CartesianGrid stroke="var(--color-line-soft)" vertical={false} />
          <XAxis
            dataKey="time"
            type="number"
            scale="time"
            domain={['dataMin', 'dataMax']}
            ticks={dayTicks(points, hoursPerTick)}
            tickFormatter={formatTick}
            tick={{ fontSize: 11, fill: 'var(--color-muted)' }}
            tickLine={false}
            axisLine={false}
          />
          <YAxis width={48} tickFormatter={(value: number) => formatNumber(value)} tick={{ fontSize: 11, fill: 'var(--color-muted)' }} tickLine={false} axisLine={false} />
          <Tooltip
            labelFormatter={(time) => formatPlantDateTime(new Date(Number(time)).toISOString())}
            formatter={(value, name) => [`${formatNumber(Number(value), 1)} kWh`, name === 'expected' ? 'Esperado' : 'Medido']}
          />
          {anomalyWindow && (
            <ReferenceArea x1={Date.parse(anomalyWindow.start)} x2={Date.parse(anomalyWindow.end)} fill="var(--color-critical)" fillOpacity={0.08} ifOverflow="hidden" />
          )}
          <Area type="monotone" dataKey="consumption" stroke="var(--color-accent)" strokeWidth={1.4} fill="var(--color-accent)" fillOpacity={0.12} isAnimationActive={false} />
          <Line type="monotone" dataKey="expected" stroke="var(--color-baseline)" strokeDasharray="4 3" strokeWidth={1.4} dot={false} isAnimationActive={false} />
          <Scatter dataKey="suspectConsumption" fill="var(--color-quality)" isAnimationActive={false} tooltipType="none" />
          {events.map((event) => (
            <ReferenceLine
              key={event.timestamp}
              x={Date.parse(event.timestamp)}
              stroke={eventTypeColor(event.type)}
              strokeDasharray="4 3"
              ifOverflow="hidden"
              label={{ value: eventTypeLabel(event.type), position: 'insideTopRight', fontSize: 11, fill: eventTypeColor(event.type) }}
            />
          ))}
        </ComposedChart>
      </ResponsiveContainer>
    </div>
  )
}
