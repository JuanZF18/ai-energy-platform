const plantTimeZone = 'America/Bogota'

const numberFormat = (decimals: number) =>
  new Intl.NumberFormat('es-CO', { minimumFractionDigits: decimals, maximumFractionDigits: decimals })

export function formatNumber(value: number, decimals = 0) {
  return numberFormat(decimals).format(value)
}

export function formatKwh(value: number | null | undefined, decimals = 0) {
  return value === null || value === undefined ? '—' : `${formatNumber(value, decimals)} kWh`
}

export function formatSignedPercent(value: number | null | undefined, decimals = 1) {
  if (value === null || value === undefined) return '—'
  const sign = value > 0 ? '+' : value < 0 ? '−' : ''
  return `${sign}${formatNumber(Math.abs(value), decimals)}%`
}

export function formatConfidence(value: number | null | undefined) {
  return value === null || value === undefined ? '—' : `${formatNumber(value * 100)}%`
}

function dateParts(iso: string, timeZone?: string) {
  const parts = new Intl.DateTimeFormat('es-CO', {
    timeZone,
    day: 'numeric',
    month: 'numeric',
    hour: 'numeric',
    minute: 'numeric',
    hourCycle: 'h23',
  }).formatToParts(new Date(iso))
  const pick = (type: string) => (parts.find((part) => part.type === type)?.value ?? '').padStart(2, '0')
  return { day: pick('day'), month: pick('month'), hour: pick('hour'), minute: pick('minute') }
}

function dateTimeText(iso: string, timeZone?: string) {
  const { day, month, hour, minute } = dateParts(iso, timeZone)
  const hour24 = Number(hour)
  const hour12 = hour24 % 12 === 0 ? 12 : hour24 % 12
  return `${day}/${month} ${hour12}:${minute} ${hour24 < 12 ? 'AM' : 'PM'}`
}

export function formatPlantDateTime(iso: string | null | undefined) {
  return iso ? dateTimeText(iso, plantTimeZone) : '—'
}

export function formatPlantDay(iso: string | null | undefined) {
  if (!iso) return '—'
  const { day, month } = dateParts(iso, plantTimeZone)
  return `${day}/${month}`
}

export function formatDayLabel(day: string) {
  const [, month, date] = day.split('-')
  return `${date}/${month}`
}

export function formatLocalDateTime(iso: string | null | undefined) {
  return iso ? dateTimeText(iso) : '—'
}

export function formatSeconds(seconds: number | null | undefined) {
  return seconds === null || seconds === undefined ? '—' : `${formatNumber(seconds, 1)} s`
}

export function countLabel(count: number, singular: string, plural: string) {
  return `${formatNumber(count)} ${count === 1 ? singular : plural}`
}
