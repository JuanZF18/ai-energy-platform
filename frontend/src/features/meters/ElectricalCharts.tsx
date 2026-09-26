import { ComposedChart, Line, ReferenceArea, ResponsiveContainer, Scatter, Tooltip, XAxis, YAxis } from 'recharts'
import { formatNumber, formatPlantDateTime } from '@/lib/format'
import type { EvidenceWindow, VariableChange } from '@/lib/types'
import type { ChartPoint } from './chartData'

interface ElectricalVariable {
  key: 'voltage' | 'current' | 'powerFactor'
  suspectKey?: 'suspectVoltage' | 'suspectPowerFactor'
  label: string
  unit: string
  decimals: number
}

const variables: ElectricalVariable[] = [
  { key: 'voltage', suspectKey: 'suspectVoltage', label: 'Voltaje', unit: 'V', decimals: 1 },
  { key: 'current', label: 'Corriente', unit: 'A', decimals: 0 },
  { key: 'powerFactor', suspectKey: 'suspectPowerFactor', label: 'Factor de potencia', unit: '', decimals: 2 },
]

interface ElectricalChartsProps {
  points: ChartPoint[]
  anomalyWindow?: EvidenceWindow
  changes?: VariableChange[]
}

export function ElectricalCharts({ points, anomalyWindow, changes }: ElectricalChartsProps) {
  return (
    <div className="grid gap-3 md:grid-cols-3">
      {variables.map((variable) => {
        const change = changes?.find((item) => item.variable === variable.label)
        return (
          <div key={variable.key} className="grid gap-1">
            <div className="flex justify-between gap-2 text-xs">
              <b className="font-semibold">{variable.label}</b>
              {change && (
                <span className={`font-mono ${Math.abs(change.changePercent) >= 10 ? 'text-critical' : 'text-muted'}`}>
                  {formatNumber(change.before, variable.decimals)} → {formatNumber(change.after, variable.decimals)} {variable.unit}
                </span>
              )}
            </div>
            <div className="h-[84px]">
              <ResponsiveContainer width="100%" height="100%">
                <ComposedChart data={points} margin={{ top: 4, right: 4, bottom: 4, left: 4 }}>
                  <XAxis dataKey="time" type="number" domain={['dataMin', 'dataMax']} hide />
                  <YAxis domain={['auto', 'auto']} hide />
                  <Tooltip
                    labelFormatter={(time) => formatPlantDateTime(new Date(Number(time)).toISOString())}
                    formatter={(value) => [`${formatNumber(Number(value), variable.decimals)} ${variable.unit}`, variable.label]}
                  />
                  {anomalyWindow && <ReferenceArea x1={Date.parse(anomalyWindow.start)} x2={Date.parse(anomalyWindow.end)} fill="var(--color-critical)" fillOpacity={0.08} ifOverflow="hidden" />}
                  <Line type="monotone" dataKey={variable.key} stroke="var(--color-muted)" strokeWidth={1} dot={false} isAnimationActive={false} />
                  {variable.suspectKey && <Scatter dataKey={variable.suspectKey} fill="var(--color-quality)" isAnimationActive={false} />}
                </ComposedChart>
              </ResponsiveContainer>
            </div>
          </div>
        )
      })}
    </div>
  )
}
