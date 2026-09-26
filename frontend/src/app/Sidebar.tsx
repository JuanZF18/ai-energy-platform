import { useQuery } from '@tanstack/react-query'
import { Gauge, LayoutDashboard, LogOut, TriangleAlert } from 'lucide-react'
import type { LucideIcon } from 'lucide-react'
import { NavLink } from 'react-router'
import { Brand } from '@/components/BrandMark'
import { useAuth } from '@/features/auth/AuthContext'
import { api } from '@/lib/api'
import { queryKeys } from '@/lib/queryKeys'

interface NavigationItem {
  to: string
  label: string
  icon: LucideIcon
  end?: boolean
}

export const navigationItems: NavigationItem[] = [
  { to: '/', label: 'Panel general', icon: LayoutDashboard, end: true },
  { to: '/meters', label: 'Medidores', icon: Gauge },
  { to: '/anomalies', label: 'Anomalías IA', icon: TriangleAlert },
]

export function Sidebar() {
  const { session, signOut } = useAuth()
  const dashboard = useQuery({ queryKey: queryKeys.dashboard, queryFn: api.dashboard })
  const activeAnomalies = dashboard.data?.anomalies.anomalies ?? 0

  return (
    <aside className="hidden w-52 shrink-0 flex-col gap-1 bg-nav px-3 py-4 text-nav-ink md:flex">
      <div className="px-2 pb-5">
        <Brand />
      </div>
      <nav aria-label="Principal" className="grid gap-1">
        {navigationItems.map(({ to, label, icon: Icon, end }) => (
          <NavLink
            key={to}
            to={to}
            end={end}
            className={({ isActive }) =>
              `flex items-center gap-2.5 rounded-lg px-2.5 py-2 font-medium ${isActive ? 'bg-nav-active text-white' : 'hover:bg-nav-active/60 hover:text-white'}`
            }
          >
            <Icon className="size-[18px] opacity-85" aria-hidden />
            {label}
            {to === '/anomalies' && activeAnomalies > 0 && (
              <span className="ml-auto rounded-full bg-critical px-1.5 font-mono text-[10.5px] font-bold text-white" title={`${activeAnomalies} anomalías por atender`} aria-label={`${activeAnomalies} anomalías por atender`}>
                {activeAnomalies}
              </span>
            )}
          </NavLink>
        ))}
      </nav>
      <div className="mt-auto flex items-center gap-2.5 border-t border-white/10 px-2 pt-3 text-xs">
        <span className="grid size-7 place-items-center rounded-full bg-[#2b5566] text-[11px] font-bold text-white">OD</span>
        <div className="grid">
          <span className="text-white">{session?.name}</span>
          <button type="button" onClick={signOut} className="flex items-center gap-1 text-left opacity-75 hover:opacity-100">
            <LogOut className="size-3" aria-hidden />
            Cerrar sesión
          </button>
        </div>
      </div>
    </aside>
  )
}
