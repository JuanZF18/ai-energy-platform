interface SparklineProps {
  values: number[]
  color: string
  width?: number
  height?: number
}

export function Sparkline({ values, color, width = 120, height = 26 }: SparklineProps) {
  if (values.length < 2) return null

  const minimum = Math.min(...values) * 0.95
  const maximum = Math.max(...values) * 1.03
  const x = (index: number) => 2 + ((width - 6) * index) / (values.length - 1)
  const y = (value: number) => 3 + (height - 6) * (1 - (value - minimum) / (maximum - minimum || 1))
  const path = values.map((value, index) => `${index ? 'L' : 'M'}${x(index).toFixed(1)} ${y(value).toFixed(1)}`).join('')
  const last = values.length - 1

  return (
    <svg width={width} height={height} viewBox={`0 0 ${width} ${height}`} aria-hidden>
      <path d={path} fill="none" stroke={color} strokeWidth={1.5} strokeLinejoin="round" />
      <circle cx={x(last)} cy={y(values[last])} r={2.6} fill={color} />
    </svg>
  )
}
