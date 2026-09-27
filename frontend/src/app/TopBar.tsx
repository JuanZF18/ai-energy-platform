import { useQuery } from '@tanstack/react-query'
import { LogOut } from 'lucide-react'
import { BrandMark } from '@/components/BrandMark'
import { RunAnalysisButton } from '@/features/analysis/RunAnalysisButton'
import { useAuth } from '@/features/auth/AuthContext'
import { api } from '@/lib/api'
import { formatLocalDateTime } from '@/lib/format'
import { queryKeys } from '@/lib/queryKeys'
import { MeterSearch } from './MeterSearch'

export function TopBar() {
  return (
    <header className="sticky top-0 z-30 border-b border-line bg-surface">
      <div className="flex h-16 items-center gap-2 px-3 md:h-14 md:gap-3 md:px-5">
        <div className="shrink-0 md:hidden">
          <BrandMark size={30} />
        </div>
        <MeterSearch />
        <div className="ml-auto flex shrink-0 items-center gap-2 md:gap-3">
          <LastAnalysisChip />
          <RunAnalysisButton />
          <MobileSignOutButton />
        </div>
      </div>
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

  if (dashboard.isError) {
    return (
      <span className="hidden items-center gap-2 rounded-lg border border-line px-2.5 py-1.5 text-xs text-critical lg:flex">
        <span className="size-2 rounded-full bg-critical" aria-hidden />
        Sin conexión con la API
      </span>
    )
  }

  return (
    <span className="hidden items-center gap-2 rounded-lg border border-line px-2.5 py-1.5 text-xs text-muted lg:flex">
      <span className={`size-2 rounded-full ${lastAnalysis ? statusDot[lastAnalysis.status] : 'bg-line'}`} aria-hidden />
      {dashboard.isPending ? 'Cargando…' : lastAnalysis ? `Último análisis ${formatLocalDateTime(lastAnalysis.finishedAt ?? lastAnalysis.requestedAt)}` : 'Sin análisis todavía'}
    </span>
  )
}

function MobileSignOutButton() {
  const { signOut } = useAuth()

  return (
    <button
      type="button"
      onClick={signOut}
      className="grid size-11 place-items-center rounded-lg border border-critical/30 bg-critical-soft text-critical hover:bg-critical hover:text-white md:hidden"
      aria-label="Cerrar sesión"
      title="Cerrar sesión"
    >
      <LogOut className="size-5" aria-hidden />
    </button>
  )
}
