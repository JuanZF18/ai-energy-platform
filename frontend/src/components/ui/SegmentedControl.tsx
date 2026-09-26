interface SegmentOption<T extends string> {
  value: T
  label: string
  count?: number
}

interface SegmentedControlProps<T extends string> {
  label: string
  options: SegmentOption<T>[]
  value: T
  onChange: (value: T) => void
}

export function SegmentedControl<T extends string>({ label, options, value, onChange }: SegmentedControlProps<T>) {
  return (
    <div role="radiogroup" aria-label={label} className="inline-flex max-w-full overflow-x-auto rounded-lg border border-line bg-surface">
      {options.map((option) => {
        const isSelected = option.value === value
        return (
          <button
            key={option.value}
            type="button"
            role="radio"
            aria-checked={isSelected}
            onClick={() => onChange(option.value)}
            className={`flex shrink-0 items-center gap-1.5 whitespace-nowrap border-r border-line px-3 py-1.5 text-[13px] last:border-r-0 ${isSelected ? 'bg-accent-soft font-semibold text-accent' : 'font-medium text-ink hover:bg-surface-2'}`}
          >
            {option.label}
            {option.count !== undefined && <span className="font-mono text-[11px] text-muted">{option.count}</span>}
          </button>
        )
      })}
    </div>
  )
}
