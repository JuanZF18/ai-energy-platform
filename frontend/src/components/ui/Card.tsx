import type { ReactNode } from 'react'

interface CardProps {
  children: ReactNode
  className?: string
}

export function Card({ children, className = '' }: CardProps) {
  return <div className={`grid min-w-0 content-start gap-3 rounded-xl border border-line bg-surface p-4 ${className}`}>{children}</div>
}

interface CardHeaderProps {
  title: string
  aside?: ReactNode
  action?: ReactNode
}

export function CardHeader({ title, aside, action }: CardHeaderProps) {
  return (
    <div className="flex flex-wrap items-center gap-x-3 gap-y-1">
      <h3 className="text-[15px] font-bold">{title}</h3>
      {aside && <span className="text-xs text-muted">{aside}</span>}
      {action && <div className="ml-auto">{action}</div>}
    </div>
  )
}
