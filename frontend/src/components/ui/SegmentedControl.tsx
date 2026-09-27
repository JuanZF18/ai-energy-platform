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
  const usesMobileGrid = options.length >= 4
  const layout = usesMobileGrid ? 'grid w-full grid-cols-2 sm:inline-flex sm:w-auto' : 'inline-flex max-w-full flex-wrap'

  return (
    <div role="radiogroup" aria-label={label} className={`${layout} gap-px overflow-hidden rounded-lg border border-line bg-line`}>
      {options.map((option, index) => {
        const isSelected = option.value === value
        const spansRow = usesMobileGrid && options.length % 2 === 1 && index === 0
        return (
          <button
            key={option.value}
            type="button"
            role="radio"
            aria-checked={isSelected}
            onClick={() => onChange(option.value)}
            className={`flex grow items-center justify-center gap-1.5 whitespace-nowrap px-3 py-2.5 text-sm sm:grow-0 sm:py-1.5 sm:text-[13px] ${spansRow ? 'col-span-2' : ''} ${isSelected ? 'bg-accent-soft font-semibold text-accent' : 'bg-surface font-medium text-ink hover:bg-surface-2'}`}
          >
            {option.label}
            {option.count !== undefined && <span className="font-mono text-xs text-muted">{option.count}</span>}
          </button>
        )
      })}
    </div>
  )
}
