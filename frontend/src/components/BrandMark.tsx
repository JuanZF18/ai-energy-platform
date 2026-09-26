export function BrandMark({ size = 26 }: { size?: number }) {
  return (
    <svg width={size} height={size} viewBox="0 0 26 26" aria-hidden>
      <rect width="26" height="26" rx="7" fill="#1A6C8C" />
      <path d="M14.5 4 8 14.5h5L11.5 22 18 11.5h-5z" fill="#fff" />
    </svg>
  )
}

export function Brand({ tone = 'light' }: { tone?: 'light' | 'dark' }) {
  return (
    <div className="flex items-center gap-2.5">
      <BrandMark />
      <span className="grid leading-tight">
        <span className={`font-display text-[17px] font-extrabold ${tone === 'light' ? 'text-white' : 'text-ink'}`}>Vatio</span>
        <span className={`text-[10.5px] ${tone === 'light' ? 'text-nav-ink' : 'text-muted'}`}>Energy Intelligence</span>
      </span>
    </div>
  )
}
