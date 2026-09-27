import { useQuery } from '@tanstack/react-query'
import { NavLink } from 'react-router'
import { api } from '@/lib/api'
import { queryKeys } from '@/lib/queryKeys'
import { navigationItems } from './Sidebar'

export function MobileNav() {
  const dashboard = useQuery({ queryKey: queryKeys.dashboard, queryFn: api.dashboard })
  const priorityAnomalies = dashboard.data?.anomalies.highPriority ?? 0

  return (
    <nav aria-label="Principal" className="fixed inset-x-0 bottom-0 z-40 grid grid-cols-3 border-t border-line bg-surface pb-[env(safe-area-inset-bottom)] shadow-[0_-4px_16px_rgb(15_38_49/0.08)] md:hidden">
      {navigationItems.map(({ to, label, icon: Icon, end }) => (
        <NavLink
          key={to}
          to={to}
          end={end}
          className={({ isActive }) =>
            `relative grid h-16 place-items-center content-center gap-1 text-xs font-semibold ${isActive ? 'text-accent' : 'text-muted'}`
          }
        >
          {({ isActive }) => (
            <>
              {isActive && <span className="absolute inset-x-6 top-0 h-[3px] rounded-b bg-accent" aria-hidden />}
              <span className="relative">
                <Icon className="size-6" strokeWidth={isActive ? 2.4 : 2} aria-hidden />
                {to === '/anomalies' && priorityAnomalies > 0 && (
                  <span className="absolute -top-1.5 -right-2.5 rounded-full bg-critical px-1.5 font-mono text-[10.5px] font-bold text-white" aria-label={`${priorityAnomalies} de atención prioritaria`}>
                    {priorityAnomalies}
                  </span>
                )}
              </span>
              {label}
            </>
          )}
        </NavLink>
      ))}
    </nav>
  )
}
