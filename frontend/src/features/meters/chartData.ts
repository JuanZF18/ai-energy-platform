import { formatPlantDay } from '@/lib/format'
import type { ReadingPoint } from '@/lib/types'

export interface ChartPoint {
  time: number
  consumption: number
  expected: number | null
  voltage: number
  current: number
  powerFactor: number
  suspectConsumption: number | null
  suspectVoltage: number | null
  suspectPowerFactor: number | null
}

export function toChartPoints(readings: ReadingPoint[]): ChartPoint[] {
  return readings.map((reading) => ({
    time: Date.parse(reading.timestamp),
    consumption: reading.consumptionKwh,
    expected: reading.expectedKwh,
    voltage: reading.voltageV,
    current: reading.currentA,
    powerFactor: reading.powerFactor,
    suspectConsumption: reading.isSuspect ? reading.consumptionKwh : null,
    suspectVoltage: reading.isSuspect ? reading.voltageV : null,
    suspectPowerFactor: reading.isSuspect ? reading.powerFactor : null,
  }))
}

export function dayTicks(points: ChartPoint[], everyHours: number) {
  return points.filter((_, index) => index % everyHours === 0).map((point) => point.time)
}

export function formatTick(time: number) {
  return formatPlantDay(new Date(time).toISOString())
}
