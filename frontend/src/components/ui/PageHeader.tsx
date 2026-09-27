import type { ReactNode } from 'react'

interface PageHeaderProps {
  eyebrow?: ReactNode
  title: ReactNode
  description?: ReactNode
  actions?: ReactNode
}

export function PageHeader({ eyebrow, title, description, actions }: PageHeaderProps) {
  return (
    <div className="flex flex-wrap items-end gap-3">
      <div className="grid gap-0.5">
        {eyebrow && <p className="text-sm font-semibold text-accent">{eyebrow}</p>}
        <h1 className="text-[22px] font-bold">{title}</h1>
        {description && <p className="text-[13px] text-muted">{description}</p>}
      </div>
      {actions && <div className="ml-auto flex flex-wrap gap-2">{actions}</div>}
    </div>
  )
}
