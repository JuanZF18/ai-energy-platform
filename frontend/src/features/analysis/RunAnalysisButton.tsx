import { LoaderCircle, Play } from 'lucide-react'
import { Button } from '@/components/ui/Button'
import { useAnalysis } from './AnalysisContext'

export function RunAnalysisButton({ size = 'md' }: { size?: 'sm' | 'md' | 'lg' }) {
  const { isRunning, isStarting, startAnalysis, openPanel } = useAnalysis()
  const isBusy = isRunning || isStarting

  return (
    <Button size={size} onClick={isRunning ? openPanel : startAnalysis} disabled={isStarting}>
      {isBusy ? <LoaderCircle className="size-4 animate-spin" aria-hidden /> : <Play className="size-3.5 fill-current" aria-hidden />}
      {isBusy ? 'Analizando…' : (
        <span>
          Analizar<span className="hidden sm:inline"> con IA</span>
        </span>
      )}
    </Button>
  )
}
