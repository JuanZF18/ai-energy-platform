import { useQuery } from '@tanstack/react-query'
import { NavLink } from 'react-router'
import { BrandMark } from '@/components/BrandMark'
import { RunAnalysisButton } from '@/features/analysis/RunAnalysisButton'
import { api } from '@/lib/api'
import { formatLocalDateTime } from '@/lib/format'
import { queryKeys } from '@/lib/queryKeys'
import { MeterSearch } from './MeterSearch'
import { navigationItems } from './Sidebar'

export function TopBar() {
  return (
    <header className="sticky top-0 z-30 border-b border-line bg-surface">
      <div className="flex h-14 items-center gap-3 px-4 md:px-5">
        <div className="md:hidden">
          <BrandMark />
        </div>
        <div className="ml-auto flex items-center gap-3">
          <MeterSearch />
          <LastAnalysisChip />
          <RunAnalysisButton />
        </div>
      </div>
      <nav aria-label="Principal" className="flex gap-1 overflow-x-auto border-t border-line px-3 py-1.5 md:hidden">
        {navigationItems.map(({ to, label, end }) => (
          <NavLink
            key={to}
            to={to}
            end={end}
            className={({ isActive }) => `rounded-md px-3 py-1.5 text-[13px] font-medium ${isActive ? 'bg-accent-soft text-accent' : 'text-muted'}`}
          >
            {label}
          </NavLink>
        ))}
      </nav>
    </header>
  )
}

const statusDot: Record<string, string> = {
  COMPLETED: 'bg-ok',
  FAILED: 'bg-critical',
  RUNNING: 'bg-accent animate-pulse',
  QUEUED: 'bg-accent animate-pulse',
}

function LastAnalysisChip() {
  const dashboard = useQuery({ queryKey: queryKeys.dashboard, queryFn: api.dashboard })
  const lastAnalysis = dashboard.data?.lastAnalysis

  return (
    <span className="hidden items-center gap-2 rounded-lg border border-line px-2.5 py-1.5 text-xs text-muted lg:flex">
      <span className={`size-2 rounded-full ${lastAnalysis ? statusDot[lastAnalysis.status] : 'bg-line'}`} aria-hidden />
      {lastAnalysis ? `Último análisis ${formatLocalDateTime(lastAnalysis.finishedAt ?? lastAnalysis.requestedAt)}` : 'Sin análisis todavía'}
    </span>
  )
}
