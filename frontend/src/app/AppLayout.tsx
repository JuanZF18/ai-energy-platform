import { Suspense } from 'react'
import { Outlet } from 'react-router'
import { LoadingBlock } from '@/components/ui/States'
import { MobileNav } from './MobileNav'
import { Sidebar } from './Sidebar'
import { TopBar } from './TopBar'

export function AppLayout() {
  return (
    <div className="flex min-h-full">
      <Sidebar />
      <div className="flex min-w-0 flex-1 flex-col">
        <TopBar />
        <main className="mx-auto grid w-full max-w-[1400px] grid-cols-[minmax(0,1fr)] content-start gap-4 p-4 pb-24 md:p-5">
          <Suspense fallback={<LoadingBlock rows={6} />}>
            <Outlet />
          </Suspense>
        </main>
      </div>
      <MobileNav />
    </div>
  )
}
