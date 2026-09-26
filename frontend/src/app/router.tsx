import { lazy } from 'react'
import { createBrowserRouter } from 'react-router'
import { AnalysisProvider } from '@/features/analysis/AnalysisContext'
import { LoginPage } from '@/features/auth/LoginPage'
import { RequireAuth } from '@/features/auth/RequireAuth'
import { AppLayout } from './AppLayout'
import { NotFoundPage } from './NotFoundPage'

const DashboardPage = lazy(() => import('@/features/dashboard/DashboardPage'))
const MetersPage = lazy(() => import('@/features/meters/MetersPage'))
const MeterDetailPage = lazy(() => import('@/features/meters/MeterDetailPage'))
const AnomaliesPage = lazy(() => import('@/features/anomalies/AnomaliesPage'))
const InvestigationPage = lazy(() => import('@/features/anomalies/InvestigationPage'))

export const router = createBrowserRouter([
  { path: '/login', element: <LoginPage /> },
  {
    element: (
      <RequireAuth>
        <AnalysisProvider>
          <AppLayout />
        </AnalysisProvider>
      </RequireAuth>
    ),
    children: [
      { index: true, element: <DashboardPage /> },
      { path: 'meters', element: <MetersPage /> },
      { path: 'meters/:meterId', element: <MeterDetailPage /> },
      { path: 'anomalies', element: <AnomaliesPage /> },
      { path: 'anomalies/:anomalyId', element: <InvestigationPage /> },
      { path: '*', element: <NotFoundPage /> },
    ],
  },
])
